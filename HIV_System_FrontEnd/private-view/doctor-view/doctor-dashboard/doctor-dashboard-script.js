// Get doctorId from localStorage (stored as accId)
const doctorId = localStorage.getItem('accId');

// Function to fetch doctor's work schedule with JWT from localStorage
async function getWorkSchedule(doctorId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedules?doctorId=${doctorId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            throw new Error('Failed to fetch work schedule');
        }
        const data = await response.json();
        return data; // Expecting an array of schedules
    } catch (error) {
        console.error('Error fetching work schedule:', error);
        return null;
    }
}

// Helper to convert dayOfWeek to day name
function getDayName(dayOfWeek) {
    const days = [null, "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
    return days[dayOfWeek] || "Unknown";
}

// Helper to get the start of the week (Monday)
function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay() || 7; // Sunday is 0, set to 7
    if (day !== 1) d.setHours(-24 * (day - 1));
    d.setHours(0, 0, 0, 0);
    return d;
}

// Helper to get the end of the week (Sunday)
function getWeekEnd(date) {
    const start = getWeekStart(date);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return end;
}

// Helper to format date as yyyy-mm-dd
function formatDate(date) {
    return date.toISOString().split('T')[0];
}

// Group schedules by week start date
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

// Helper: get all days in a week (Monday to Sunday)
function getWeekDays(weekStart) {
    const days = [];
    for (let i = 0; i < 7; i++) {
        const d = new Date(weekStart);
        d.setDate(d.getDate() + i);
        days.push(d);
    }
    return days;
}

// Render the week dropdown and schedule calendar grid (timeframes as rows)
async function renderWorkSchedule(doctorId, containerId) {
    const container = document.getElementById(containerId);
    const tableContainer = document.getElementById('scheduleTableContainer');
    container.querySelector('#weekSelector').innerHTML = '<option>Loading...</option>';
    tableContainer.innerHTML = "<p>Loading work schedule...</p>";

    const schedules = await getWorkSchedule(doctorId);
    if (!schedules || schedules.length === 0) {
        tableContainer.innerHTML = "<p>No work schedule found.</p>";
        container.querySelector('#weekSelector').innerHTML = '';
        return;
    }

    // Group by week
    const weeks = groupSchedulesByWeek(schedules);
    const weekKeys = Object.keys(weeks).sort();

    // Populate dropdown
    const weekSelector = container.querySelector('#weekSelector');
    weekSelector.innerHTML = weekKeys.map(weekKey => {
        const [start, end] = weekKey.split('~');
        return `<option value="${weekKey}">${start} to ${end}</option>`;
    }).join('');

    // Render calendar for selected week
    function renderCalendar(weekKey) {
        const weekSchedules = weeks[weekKey] || [];
        if (weekSchedules.length === 0) {
            tableContainer.innerHTML = "<p>No work schedule for this week.</p>";
            return;
        }

        // 1. Get all unique timeframes for this week
        const timeframeSet = new Set();
        weekSchedules.forEach(item => {
            timeframeSet.add(`${item.startTime}-${item.endTime}`);
        });
        // Sort timeframes by startTime
        const timeframes = Array.from(timeframeSet).sort((a, b) => {
            const aStart = a.split('-')[0];
            const bStart = b.split('-')[0];
            return aStart.localeCompare(bStart);
        });

        // 2. Build a map: { [dayOfWeek][timeframe]: schedule }
        const scheduleMap = {};
        weekSchedules.forEach(item => {
            const tfKey = `${item.startTime}-${item.endTime}`;
            if (!scheduleMap[item.dayOfWeek]) scheduleMap[item.dayOfWeek] = {};
            scheduleMap[item.dayOfWeek][tfKey] = item;
        });

        // 3. Render table
        let html = `<table class="calendar-table" style="width:100%;border-collapse:collapse;text-align:center;">
            <thead>
                <tr>
                    <th style="background:#f3f4f6;">Timeframe</th>
                    ${[1,2,3,4,5,6,7].map(dayNum => `<th style="background:#f3f4f6;">${getDayName(dayNum)}</th>`).join('')}
                </tr>
            </thead>
            <tbody>
                ${timeframes.map(tf => {
                    const [start, end] = tf.split('-');
                    return `<tr>
                        <td style="font-weight:bold;">${start.slice(0,5)} - ${end.slice(0,5)}</td>
                        ${[1,2,3,4,5,6,7].map(dayNum => {
                            const sched = (scheduleMap[dayNum] && scheduleMap[dayNum][tf]) ? scheduleMap[dayNum][tf] : null;
                            if (sched) {
                                return `<td style="background:#e0f7fa;padding:8px;">
                                    <div>${sched.isAvailable ? '<span style="color:green;">Available</span>' : '<span style="color:red;">Not Available</span>'}</div>
                                    <div>${sched.workDate ? new Date(sched.workDate).toLocaleDateString() : ''}</div>
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

    // Initial render
    renderCalendar(weekSelector.value);

    // Change event
    weekSelector.onchange = () => renderCalendar(weekSelector.value);
}

// Function to fetch doctor profile (excluding workSchedule)
async function getDoctorProfile(accId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/Doctor/GetDoctorProfile?accId=${accId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch doctor profile');
        return await response.json();
    } catch (error) {
        console.error('Error fetching doctor profile:', error);
        return null;
    }
}

// Render doctor profile (excluding workSchedule)
async function renderDoctorProfile(accId, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = "<p>Loading profile...</p>";
    const profile = await getDoctorProfile(accId);
    if (profile) {
        container.innerHTML = `
            <div class="profile-header">
                <h2>${profile.account.fullname}</h2>
                <p>${profile.account.email}</p>
                <p>${profile.degree}</p>
                <p>${profile.bio}</p>
            </div>
        `;
    } else {
        container.innerHTML = "<p>Failed to load profile.</p>";
    }
}

// Navigation and event logic
function setActive(btn) {
    const btnSchedule = document.getElementById('btnSchedule');
    const btnProfile = document.getElementById('btnProfile');
    [btnSchedule, btnProfile].forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
}

function logout() {
    window.location.href = "../../../public-view/landingpage.html";
}

// Attach event listeners after DOM is loaded
window.addEventListener('DOMContentLoaded', () => {
    const btnSchedule = document.getElementById('btnSchedule');
    const btnProfile = document.getElementById('btnProfile');
    const logoutBtn = document.getElementById('logoutBtn');
    const dashboardHeader = document.getElementById('dashboardHeader');

    btnSchedule.onclick = function() {
        setActive(btnSchedule);
        dashboardHeader.textContent = "Work Schedule";
        renderWorkSchedule(doctorId, 'dashboardSection');
    };

    btnProfile.onclick = function() {
        window.location.href = "../doctor-profile/doctor-profile.html";
    };

    if (logoutBtn) logoutBtn.onclick = logout;

    // Load schedule by default
    btnSchedule.click();
});