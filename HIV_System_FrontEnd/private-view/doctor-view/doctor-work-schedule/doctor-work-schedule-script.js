// Lấy doctorId từ localStorage (lưu dưới dạng accId)
const doctorId = localStorage.getItem('accId');

// Hàm lấy lịch làm việc của bác sĩ với JWT từ localStorage
async function getWorkSchedule(doctorId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetPersonalWorkSchedules`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            throw new Error('Không thể lấy lịch làm việc');
        }
        const data = await response.json();
        return data; // Trả về mảng lịch làm việc
    } catch (error) {
        console.error('Lỗi khi lấy lịch làm việc:', error);
        return null;
    }
}

// Chuyển dayOfWeek sang tên thứ tiếng Việt
function getDayName(dayOfWeek) {
    const days = [null, "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật"];
    return days[dayOfWeek] || "Không xác định";
}

// Lấy ngày đầu tuần (Thứ Hai)
function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay() || 7; // Chủ nhật là 0, chuyển thành 7
    if (day !== 1) d.setHours(-24 * (day - 1));
    d.setHours(0, 0, 0, 0);
    return d;
}

// Lấy ngày cuối tuần (Chủ nhật)
function getWeekEnd(date) {
    const start = getWeekStart(date);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return end;
}

// Định dạng ngày yyyy-mm-dd
function formatDate(date) {
    return date.toISOString().split('T')[0];
}

// Gom lịch làm việc theo tuần
function groupSchedulesByWeek(schedules) {
    const weeks = {};
    schedules.forEach(item => {
        const workDate = new Date(item.workDate);
        const weekStart = formatDate(getWeekStart(workDate));
        const weekEnd = formatDate(getWeekEnd(workDate));
        const weekKey = `${weekStart}~${weekEnd}`;
        if (!weeks[weekKey]) weeks[weekKey] = [];
        weeks[weekKey].push(item);
    });
    return weeks;
}

// Helper: Convert "HH:mm:ss" or "HH:mm" to minutes
function timeToMinutes(timeStr) {
    const [h, m] = timeStr.split(':');
    return parseInt(h, 10) * 60 + parseInt(m, 10);
}

// Helper: Convert minutes to "HH:mm"
function minutesToTime(mins) {
    const h = Math.floor(mins / 60).toString().padStart(2, '0');
    const m = (mins % 60).toString().padStart(2, '0');
    return `${h}:${m}`;
}

// Generate all 1h30m slots with 15m gap from all schedules
function getAllHourSlots(schedules) {
    const slotSet = new Set();
    schedules.forEach(item => {
        let start = timeToMinutes(item.startTime);
        let end = timeToMinutes(item.endTime);
        console.log('Tạo slot từ:', minutesToTime(start), 'đến', minutesToTime(end));
        while (start + 45 <= end) {
            const slotStart = start;
            const slotEnd = start + 45;
            slotSet.add(`${minutesToTime(slotStart)}-${minutesToTime(slotEnd)}`);
            start = slotEnd + 15;
        }
    });
    return Array.from(slotSet).sort((a, b) => {
        // Sắp xếp theo thời gian bắt đầu slot
        const [aStart] = a.split('-');
        const [bStart] = b.split('-');
        return timeToMinutes(aStart) - timeToMinutes(bStart);
    });
}
// Check if a slot is within a schedule
function slotInSchedule(slotStart, slotEnd, schedStart, schedEnd) {
    return slotStart >= schedStart && slotEnd <= schedEnd;
}

// Hiển thị dropdown tuần và bảng lịch làm việc
async function renderWorkSchedule(doctorId, containerId) {
    const container = document.getElementById(containerId);
    const tableContainer = document.getElementById('scheduleTableContainer');
    container.querySelector('#weekSelector').innerHTML = '<option>Đang tải...</option>';
    tableContainer.innerHTML = "<p>Đang tải lịch làm việc...</p>";

    const schedules = await getWorkSchedule(doctorId);
    if (!schedules || schedules.length === 0) {
        tableContainer.innerHTML = "<p>Không có lịch làm việc nào.</p>";
        container.querySelector('#weekSelector').innerHTML = '';
        return;
    }

    // Gom theo tuần
    const weeks = groupSchedulesByWeek(schedules);
    const weekKeys = Object.keys(weeks).sort();

    // === Lấy tất cả slot 1 giờ từ toàn bộ schedules ===
    const allHourSlots = getAllHourSlots(schedules);

    // Đổ dropdown tuần
    const weekSelector = container.querySelector('#weekSelector');
    weekSelector.innerHTML = weekKeys.map(weekKey => {
        const [start, end] = weekKey.split('~');
        return `<option value="${weekKey}">${start} đến ${end}</option>`;
    }).join('');

    // === Chọn tuần hiện tại mặc định ===
    const today = new Date();
    const currentWeekStart = formatDate(getWeekStart(today));
    const currentWeekEnd = formatDate(getWeekEnd(today));
    const currentWeekKey = `${currentWeekStart}~${currentWeekEnd}`;
    if (weekKeys.includes(currentWeekKey)) {
        weekSelector.value = currentWeekKey;
    } else {
        weekSelector.value = weekKeys[0];
    }

    // Hiển thị bảng lịch cho tuần đã chọn
    function renderCalendar(weekKey) {
        const weekSchedules = weeks[weekKey] || [];
        if (weekSchedules.length === 0) {
            tableContainer.innerHTML = "<p>Không có lịch làm việc cho tuần này.</p>";
            return;
        }

        // Tạo map: { [dayOfWeek]: [schedules...] }
        const scheduleMap = {};
        weekSchedules.forEach(item => {
            if (!scheduleMap[item.dayOfWeek]) scheduleMap[item.dayOfWeek] = [];
            scheduleMap[item.dayOfWeek].push(item);
        });

        // Hiển thị bảng với tất cả slot 1 giờ (allHourSlots)
        let html = `<table class="calendar-table" style="width:100%;border-collapse:collapse;text-align:center;">
            <thead>
                <tr>
                    <th style="background:#f3f4f6;">Khung giờ</th>
                    ${[1,2,3,4,5,6,7].map(dayNum => `<th style="background:#f3f4f6;">${getDayName(dayNum)}</th>`).join('')}
                </tr>
            </thead>
            <tbody>
                ${allHourSlots.map(slot => {
                    const [slotStartStr, slotEndStr] = slot.split('-');
                    const slotStart = timeToMinutes(slotStartStr);
                    const slotEnd = timeToMinutes(slotEndStr);
                    return `<tr>
                        <td style="font-weight:bold;">${slotStartStr} - ${slotEndStr}</td>
                        ${[1,2,3,4,5,6,7].map(dayNum => {
                            // Tìm schedule nào chứa slot này
                            const daySchedules = scheduleMap[dayNum] || [];
                            const sched = daySchedules.find(s =>
                                slotInSchedule(slotStart, slotEnd, timeToMinutes(s.startTime), timeToMinutes(s.endTime))
                            );
                            if (sched) {
                                return `<td style="background:#e0f7fa;padding:8px;">
                                    <div>${sched.isAvailable ? '<span style="color:green;">Lịch trống</span>' : '<span style="color:red;">Có việc bận</span>'}</div>
                                    <div>${sched.workDate ? new Date(sched.workDate).toLocaleDateString('vi-VN') : ''}</div>
                                </td>`;
                            } else {
                                return `<td style="background:#fff;padding:8px;">-</td>`;
                            }
                        }).join('')}
                    </tr>`;
                }).join('')}
            </tbody>
        </table>`;

        tableContainer.innerHTML = html;
    }

    // Hiển thị lần đầu với tuần đã chọn
    renderCalendar(weekSelector.value);

    // Sự kiện đổi tuần
    weekSelector.onchange = () => renderCalendar(weekSelector.value);
}

// Navigation và sự kiện
function setActive(btn) {
    const btnSchedule = document.getElementById('btnSchedule');
    const btnProfile = document.getElementById('btnProfile');
    [btnSchedule, btnProfile].forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
}

// Gắn sự kiện sau khi DOM đã load
window.addEventListener('DOMContentLoaded', () => {
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) logoutBtn.onclick = logout;
    // Luôn hiển thị lịch làm việc mặc định
    renderWorkSchedule(doctorId, 'dashboardSection');
});