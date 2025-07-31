// Ensure role flags are set at the very top
window.isStaff = false;
window.isDoctor = false;
if (window.roleUtils && window.roleUtils.getUserRole && window.roleUtils.ROLE_NAMES) {
    const roleId = window.roleUtils.getUserRole();
    const roleName = window.roleUtils.ROLE_NAMES[roleId];
    window.isStaff = (roleName === 'staff');
    window.isDoctor = (roleName === 'doctor');
}

// Get token from localStorage
const token = localStorage.getItem('token');

// Define section globally for use in renderAppointments and event delegation
const section = document.getElementById('appointmentListSection');

// Create filter bar only once, outside the section
if (!document.getElementById('filterStatus')) {
    const filterBar = document.createElement('div');
    filterBar.className = 'filter-bar';
    filterBar.innerHTML = `
    <label for="filterStatus">Trạng thái:</label>
    <select id="filterStatus">
        <option value="">Tất cả</option>
        <option value="1">Chờ xác nhận</option>
        <option value="2">Đã lên lịch</option>
        <option value="4">Đã hủy</option>
        <option value="5">Đã hoàn thành</option>
    </select>
    <label for="filterDate">Ngày:</label>
    <input type="date" id="filterDate">
    <label for="filterTime">Giờ:</label>
    <input type="time" id="filterTime">
    <label for="sortDate">Sắp xếp ngày:</label>
    <select id="sortDate">
        <option value="desc">Mới nhất</option>
        <option value="asc">Cũ nhất</option>
    </select>
    <button id="clearFilters">Xóa lọc</button>
    `;
    // Insert filter bar before the appointment list section
    section.parentNode.insertBefore(filterBar, section);
}

// Appointment status mapping (add virtual status for frontend display)
const appointmentStatusMap = {
    1: "Chờ xác nhận",
    2: "Đã lên lịch",
    4: "Đã hủy",
    5: "Đã hoàn thành",
};

// Helper to determine display status (virtual rejected for pending->cancelled)
function getDisplayStatus(apmt) {
    // If appointment was pending and then rejected (cancelled), mark as rejected in UI
    if (apmt.apmStatus === 4 && apmt._wasPending) {
        return { label: appointmentStatusMap[6], class: 'status-rejected' };
    }
    return { label: appointmentStatusMap[apmt.apmStatus] || 'Không rõ', class: `status-${apmt.apmStatus}` };
}

async function fetchAppointments() {
    try {
        let url = '';
        if (window.isDoctor) {
            url = 'https://localhost:7009/api/Appointment/GetAllPersonalAppointments';
        } else if (window.isStaff) {
            url = 'https://localhost:7009/api/Appointment/GetAllAppointments';
        } else {
            throw new Error('Không xác định vai trò người dùng.');
        }
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Lỗi không thể lấy lịch hẹn');
        return await response.json();
    } catch (error) {
        console.error('Lỗi không thể lấy lịch hẹn:', error);
        return [];
    }
}

function renderAppointments(appointments) {
    if (!section) return; // Ensure section is available
    if (!appointments.length) {
        section.innerHTML = '<p>Không có lịch hẹn nào.</p>';
        return;
    }
    let html = `<table class="appointment-table">
        <thead>
            <tr>
                <th>STT</th>
                <th>Bệnh nhân</th>
                <th>Ngày</th>
                <th>Giờ</th>
                <th>Ghi chú</th>
                <th>Trạng thái</th>
                <th>Hành động</th>
            </tr>
        </thead>
        <tbody>
            ${appointments.map((apmt, idx) => {
        return `<tr data-apmt-id="${apmt.appointmentId}">
                    <td>${idx + 1}</td>
                    <td>${apmt.patientName}</td>
                    <td>${apmt.apmtDate || apmt.requestDate || ''}</td>
                    <td>${apmt.apmTime || apmt.requestTime || ''}</td>
                    <td>${apmt.notes || ''}</td>
                    <td>${getDisplayStatus(apmt).label}</td>
                    <td><button class="button-view" data-view-id="${apmt.appointmentId}">Xem</button></td>
                </tr>`;
    }).join('')}
        </tbody>
    </table>`;
    section.innerHTML = html;
}

// Store all appointments for filtering
let allAppointments = [];

// Filtering logic
function applyFilters() {
    const status = document.getElementById('filterStatus').value;
    const date = document.getElementById('filterDate').value;
    const time = document.getElementById('filterTime').value;
    const sortOrder = document.getElementById('sortDate') ? document.getElementById('sortDate').value : 'desc';

    let filtered = allAppointments.filter(apmt => {
        let match = true;
        if (status && String(apmt.apmStatus) !== status) match = false;
        if (date && (apmt.apmtDate !== date && apmt.requestDate !== date)) match = false;
        if (time && ((apmt.apmTime && !apmt.apmTime.startsWith(time)) && (apmt.requestTime && !apmt.requestTime.startsWith(time)))) match = false;
        return match;
    });

    // Sort by date
    filtered.sort((a, b) => {
    // Lấy ngày và giờ, nếu thiếu thì dùng mặc định
    const dateStrA = (a.apmtDate || a.requestDate) + 'T' + (a.apmTime || a.requestTime);
    const dateStrB = (b.apmtDate || b.requestDate) + 'T' + (b.apmTime || b.requestTime);
    const dateA = new Date(dateStrA);
    const dateB = new Date(dateStrB);
    if (sortOrder === 'asc') return dateA - dateB;
    return dateB - dateA;
});

    renderAppointments(filtered);
}

// Event delegation for view button
section.addEventListener('click', async function (e) {
    const btn = e.target.closest('.button-view');
    if (btn) {
        const apmtId = btn.getAttribute('data-view-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/GetAppointmentById/${apmtId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error('Không thể lấy chi tiết lịch hẹn');
            const apmt = await res.json();
            showAppointmentDetailsModal(apmt);
        } catch (err) {
            setMessage('Lỗi khi lấy chi tiết lịch hẹn.', false);
        }
    }
});

function showAppointmentDetailsModal(apmt) {
    const modal = document.getElementById('appointmentDetailsModal');
    const content = document.getElementById('appointmentDetailsContent');
    const actions = document.getElementById('appointmentDetailsActions');
    // Render appointment info
    content.innerHTML = `
      <p><b>Bệnh nhân:</b> ${apmt.patientName}</p>
      <p><b>Bác sĩ:</b> ${apmt.doctorName}</p>
      <p><b>Ngày hẹn:</b> ${apmt.apmtDate || apmt.requestDate || ''}</p>
      <p><b>Giờ hẹn:</b> ${apmt.apmTime || apmt.requestTime || ''}</p>
      <p><b>Ghi chú:</b> ${apmt.notes || ''}</p>
      <p><b>Trạng thái:</b> ${getDisplayStatus(apmt).label}</p>
    `;
    // Render action buttons
    let btns = '';
    if (window.isDoctor) { // Only doctors can see these buttons
        if (apmt.apmStatus === 1) {
            btns += `<button class="button-accept" data-accept-id="${apmt.appointmentId}">Chấp nhận</button>`;
            btns += `<button class="button-reject" data-reject-id="${apmt.appointmentId}" data-apmt-status="${apmt.apmStatus}">Từ chối</button>`;
            btns += `<button class="button-modify" data-modify-id="${apmt.appointmentId}">Chỉnh sửa</button>`;
        }
        if (apmt.apmStatus === 2 || apmt.apmStatus === 3) {
            btns += `<button class="button-cancel" data-cancel-id="${apmt.appointmentId}">Hủy lịch</button>`;
            btns += `<button class="button-modify" data-modify-id="${apmt.appointmentId}">Chỉnh sửa</button>`;
            btns += `<button class="button-complete" data-complete-id="${apmt.appointmentId}">Hoàn thành</button>`;
        }
    }
    actions.innerHTML = btns;
    modal.style.display = 'block';

    // Accept button
    actions.querySelector('.button-accept')?.addEventListener('click', async function () {
        const apmtId = this.getAttribute('data-accept-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=2`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã được chấp nhận!', true);
            modal.style.display = 'none';
            allAppointments = await fetchAppointments();
            applyFilters();
        } catch (err) {
            let msg = err.message;
            // Try to extract error message if it's a JSON string with an 'error' property
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) { }
            setMessage('Lỗi khi chấp nhận lịch hẹn: ' + msg, false);
        }
    });
    // Reject button
    actions.querySelector('.button-reject')?.addEventListener('click', async function () {
        const apmtId = this.getAttribute('data-reject-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=4`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã bị từ chối!', true);
            modal.style.display = 'none';
            allAppointments = await fetchAppointments();
            applyFilters();
        } catch (err) {
            let msg = err.message;
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) { }
            setMessage('Lỗi khi từ chối lịch hẹn: ' + msg, false);
        }
    });
    // Cancel button
    actions.querySelector('.button-cancel')?.addEventListener('click', async function () {
        const apmtId = this.getAttribute('data-cancel-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=4`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã được hủy!', true);
            modal.style.display = 'none';
            allAppointments = await fetchAppointments();
            applyFilters();
        } catch (err) {
            let msg = err.message;
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) { }
            setMessage('Lỗi khi hủy lịch hẹn: ' + msg, false);
        }
    });
    // Complete button
    actions.querySelector('.button-complete')?.addEventListener('click', function () {
        const apmtId = this.getAttribute('data-complete-id');
        // Open complete modal and set appointment ID
        const completeModal = document.getElementById('completeAppointmentModal');
        const completeForm = document.getElementById('completeAppointmentForm');
        if (completeForm) completeForm.setAttribute('data-apmt-id', apmtId);
        if (completeModal) completeModal.style.display = 'block';
        // Hide the appointment details modal so the complete modal is upfront
        modal.style.display = 'none';
    });

    // Modify button
    actions.querySelector('.button-modify')?.addEventListener('click', function () {
        document.getElementById('modifyDate').value = apmt.apmtDate || apmt.requestDate || '';
        document.getElementById('modifyNotes').value = apmt.notes || '';
        document.getElementById('modifyAppointmentModal').style.display = 'block';
        document.getElementById('modifyAppointmentForm').setAttribute('data-apmt-id', apmt.appointmentId);
        modal.style.display = 'none';
        // Fetch and populate available time slots from work schedule
        const doctorId = apmt.doctorId;
        const date = apmt.apmtDate || apmt.requestDate || '';
        const timeSelect = document.getElementById('modifyTime');
        timeSelect.innerHTML = '<option>Đang tải...</option>';
        fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetPersonalWorkSchedules`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(res => res.json())
            .then(schedules => {
                // Chỉ lấy các ca làm việc khả dụng cho ngày đã chọn
                const slots = (schedules || []).filter(s => s.workDate === date && s.isAvailable);
                let options = [];
                slots.forEach(s => {
                    let start = s.startTime.slice(0, 5); // "HH:mm"
                    let end = s.endTime.slice(0, 5);
                    let [sh, sm] = start.split(':').map(Number);
                    let [eh, em] = end.split(':').map(Number);
                    let current = new Date(0, 0, 0, sh, sm);
                    let endTime = new Date(0, 0, 0, eh, em);

                    // Tạo các slot 1 tiếng, chỉ hiển thị HH:mm
                    while (current.getTime() + 45 * 60 * 1000 <= endTime.getTime()) {
                        let h = current.getHours().toString().padStart(2, '0');
                        let m = current.getMinutes().toString().padStart(2, '0');
                        let slotStart = `${h}:${m}`;
                        options.push(slotStart);
                        // Nhảy sang slot tiếp theo (45 phút + 15 phút nghỉ)
                        current = new Date(current.getTime() + (45 + 15) * 60 * 1000);
                    }
                });
                options = Array.from(new Set(options));
                if (options.length > 0) {
                    timeSelect.innerHTML = options.map(t => `<option value="${t}">${t}</option>`).join('');
                    // Nếu giờ hiện tại trùng slot thì chọn trước
                    const currentTime = (apmt.apmTime || apmt.requestTime || '').slice(0, 5);
                    if (options.includes(currentTime)) timeSelect.value = currentTime;
                } else {
                    timeSelect.innerHTML = '<option value="">Không có khung giờ khả dụng</option>';
                }
            })
            .catch(() => {
                timeSelect.innerHTML = '<option value="">Lỗi tải khung giờ</option>';
            });
    });
}

document.getElementById('closeAppointmentDetailsModal').onclick = function () {
    document.getElementById('appointmentDetailsModal').style.display = 'none';
};

// Main startup logic
window.addEventListener('DOMContentLoaded', async () => {
    // Fetch and render appointments
    allAppointments = await fetchAppointments();
    applyFilters();

    // Filter event listeners
    document.getElementById('filterStatus').addEventListener('change', applyFilters);
    document.getElementById('filterDate').addEventListener('change', applyFilters);
    document.getElementById('filterTime').addEventListener('change', applyFilters);
    document.getElementById('sortDate').addEventListener('change', applyFilters);
    document.getElementById('clearFilters').addEventListener('click', function () {
        document.getElementById('filterStatus').value = '';
        document.getElementById('filterDate').value = '';
        document.getElementById('filterTime').value = '';
        applyFilters();
    });

    // Modal close/cancel for complete
    const closeCompleteModalBtn = document.getElementById('closeCompleteModal');
    if (closeCompleteModalBtn) closeCompleteModalBtn.onclick = closeCompleteModal;
    const cancelCompleteBtn = document.getElementById('cancelCompleteBtn');
    if (cancelCompleteBtn) cancelCompleteBtn.onclick = closeCompleteModal;
    function closeCompleteModal() {
        const completeModal = document.getElementById('completeAppointmentModal');
        const completeForm = document.getElementById('completeAppointmentForm');
        if (completeModal && completeForm) {
            completeModal.style.display = 'none';
            completeForm.reset();
        }
    }
    // Complete appointment form
    const completeAppointmentForm = document.getElementById('completeAppointmentForm');
    if (completeAppointmentForm) {
        completeAppointmentForm.onsubmit = async function (e) {
            e.preventDefault();
            const apmtId = this.getAttribute('data-apmt-id');
            const notes = document.getElementById('completeNotes') ? document.getElementById('completeNotes').value : '';
            const submitBtn = document.getElementById('submitCompleteBtn');
            if (submitBtn) submitBtn.disabled = true;
            setMessage('Đang xử lý...', false);
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/CompleteAppointment?appointmentId=${apmtId}`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ notes })
                });
                if (!res.ok) {
                    let msg = 'Không thể hoàn thành lịch hẹn.';
                    try {
                        const data = await res.json();
                        if (data && data.error) msg = data.error;
                        else if (typeof data === 'string') msg = data;
                    } catch (e) {
                        try {
                            const text = await res.text();
                            if (text) msg = text;
                        } catch (e2) { }
                    }
                    setMessage(msg, false);
                    if (submitBtn) submitBtn.disabled = false;
                    return;
                }
                setMessage('Lịch hẹn đã được đánh dấu hoàn thành!', true);
                document.getElementById('completeAppointmentModal').style.display = 'none';
                document.getElementById('appointmentDetailsModal').style.display = 'none';
                allAppointments = await fetchAppointments();
                applyFilters();
            } catch (err) {
                setMessage('Lỗi khi hoàn thành lịch hẹn.', false);
                if (submitBtn) submitBtn.disabled = false;
            }
        };
    }

    // Only doctors can modify appointments
    if (window.isDoctor) {
        document.getElementById('closeModifyModal').onclick = closeModifyModal;
        document.getElementById('cancelModifyBtn').onclick = closeModifyModal;
        function closeModifyModal() {
            document.getElementById('modifyAppointmentModal').style.display = 'none';
            document.getElementById('modifyAppointmentForm').reset();
            document.getElementById('modifyTime').innerHTML = '';
        }
        document.getElementById('modifyAppointmentForm').onsubmit = async function (e) {
            e.preventDefault();
            const apmtId = this.getAttribute('data-apmt-id');
            const date = document.getElementById('modifyDate').value;
            let time = document.getElementById('modifyTime').value;
            const notes = document.getElementById('modifyNotes').value;
            const today = new Date();
            const selDate = new Date(date);
            today.setHours(0, 0, 0, 0);
            selDate.setHours(0, 0, 0, 0);
            if (selDate < today) {
                alert('Ngày lịch hẹn không thể trong quá khứ.');
                return;
            }
            if (!time) {
                alert('Vui lòng chọn một thời gian hợp lệ.');
                return;
            }
            if (time.length === 5) time += ':00';
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/UpdateAppointmentRequest?id=${apmtId}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        appointmentDate: date,
                        appointmentTime: time,
                        notes
                    })
                });
                if (!res.ok) {
                    let msg = 'Không thể cập nhật lịch hẹn.';
                    try {
                        const text = await res.text();
                        if (text && text.trim().length > 0) {
                            msg = text;
                        } else {
                            const data = JSON.parse(text);
                            if (data) {
                                if (data.error) msg = data.error;
                                else if (data.message) msg = data.message;
                                else if (data.title) msg = data.title;
                                else if (data.detail) msg = data.detail;
                                else if (data.errors && typeof data.errors === 'object') {
                                    const firstKey = Object.keys(data.errors)[0];
                                    if (firstKey && Array.isArray(data.errors[firstKey]) && data.errors[firstKey][0]) {
                                        msg = data.errors[firstKey][0];
                                    }
                                }
                            }
                        }
                    } catch (e) { }
                    setMessage(msg, false);
                    return;
                }
                setMessage('Lịch hẹn đã được cập nhật thành công!', true);
                document.getElementById('modifyAppointmentModal').style.display = 'none';
                document.getElementById('appointmentDetailsModal').style.display = 'none';
                allAppointments = await fetchAppointments();
                applyFilters();
            } catch (err) {
                setMessage('Lỗi khi cập nhật lịch hẹn.', false);
            }
        };

        const dateInput = document.getElementById('modifyDate');
        const timeSelect = document.getElementById('modifyTime');
        if (dateInput) {
            dateInput.onchange = function () {
                const date = dateInput.value;
                timeSelect.innerHTML = '<option>Đang tải...</option>';
                fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetPersonalWorkSchedules`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                })
                    .then(res => res.json())
                    .then(schedules => {
                        const slots = (schedules || []).filter(s => s.workDate === date && s.isAvailable);
                        let options = [];
                        slots.forEach(s => {
                            let start = s.startTime.slice(0, 5);
                            let end = s.endTime.slice(0, 5);
                            let [sh, sm] = start.split(':').map(Number);
                            let [eh, em] = end.split(':').map(Number);
                            let current = new Date(0, 0, 0, sh, sm);
                            let endTime = new Date(0, 0, 0, eh, em);
                            while (current.getTime() + 45 * 60 * 1000 <= endTime.getTime()) {
                                let h = current.getHours().toString().padStart(2, '0');
                                let m = current.getMinutes().toString().padStart(2, '0');
                                let slotStart = `${h}:${m}`;
                                options.push(slotStart);
                                // Nhảy sang slot tiếp theo (45 phút + 15 phút nghỉ)
                                current = new Date(current.getTime() + (45 + 15) * 60 * 1000);
                            }
                        });
                        options = Array.from(new Set(options));
                        if (options.length > 0) {
                            timeSelect.innerHTML = options.map(t => `<option value="${t}">${t}</option>`).join('');
                        } else {
                            timeSelect.innerHTML = '<option value="">Không có khung giờ khả dụng</option>';
                        }
                    })
                    .catch(() => {
                        timeSelect.innerHTML = '<option value="">Lỗi tải khung giờ</option>';
                    });
            };
        }
    }

    // Modal close logic for message modal
    const modal = document.getElementById('messageModal');
    const closeBtn = document.getElementById('closeMessageModal');
    if (closeBtn && modal) {
        closeBtn.onclick = function () {
            modal.classList.remove('show');
        };
        window.onclick = function (event) {
            if (event.target === modal) {
                modal.classList.remove('show');
            }
        };
    }
});

// setMessage modal
function setMessage(msg, success) {
    const modal = document.getElementById('messageModal');
    const modalText = document.getElementById('messageModalText');
    if (!modal || !modalText) return;
    modalText.textContent = msg;
    modalText.className = success ? 'modal-success' : 'modal-error';
    modal.classList.add('show');
}

// Helper to convert date to mm/dd/yyyy for display
function toDisplayDateFormat(dateStr) {
    if (!dateStr) return '';
    // yyyy-mm-dd to mm/dd/yyyy
    if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
        const [y, m, d] = dateStr.split('-');
        return `${m}/${d}/${y}`;
    }
    // dd/mm/yyyy to mm/dd/yyyy
    if (/^\d{2}\/\d{2}\/\d{4}$/.test(dateStr)) {
        const [d, m, y] = dateStr.split('/');
        return `${m}/${d}/${y}`;
    }
    return dateStr;
}
// Helper to convert to yyyy-mm-dd for input value
function toInputDateFormat(dateStr) {
    if (!dateStr) return '';
    if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) return dateStr;
    if (/^\d{2}\/\d{2}\/\d{4}$/.test(dateStr)) {
        const [d, m, y] = dateStr.split('/');
        return `${y}-${m.padStart(2, '0')}-${d.padStart(2, '0')}`;
    }
    return '';
}