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

// Appointment status mapping (add virtual status for frontend display)
const appointmentStatusMap = {
  1: "Chờ xác nhận", // Pending
  2: "Đã lên lịch",   // Scheduled
  3: "Đã lên lịch lại", // Rescheduled
  4: "Đã hủy",        // Cancelled
  5: "Đã hoàn thành", // Completed
  6: "Đã từ chối"     // Rejected (virtual, frontend only)
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
    const section = document.getElementById('appointmentListSection');
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
                let actionButtons = '';
                // Staff: Only Accept and Reject
                if (userRoleName === 'staff') {
                    if (apmt.apmStatus === 1) {
                        actionButtons += `<button class="button-accept" data-accept-id="${apmt.appointmentId}">Chấp nhận</button>`;
                    }
                    if (apmt.apmStatus !== 4 && apmt.apmStatus !== 5) {
                        // Button label based on status
                        let rejectLabel = (apmt.apmStatus === 1) ? 'Từ chối' : 'Hủy lịch';
                        actionButtons += `<button class="button-reject" data-reject-id="${apmt.appointmentId}" data-apmt-status="${apmt.apmStatus}">${rejectLabel}</button>`;
                    }
                }
                // Doctor: Accept, Modify, Reject, Complete
                else if (userRoleName === 'doctor') {
                    if (apmt.apmStatus === 1) {
                        actionButtons += `<button class="button-accept" data-accept-id="${apmt.appointmentId}">Chấp nhận</button>`;
                        actionButtons += `<button class="button-modify" data-modify-id="${apmt.appointmentId}">Chỉnh sửa</button>`;
                    }
                    if (apmt.apmStatus !== 4 && apmt.apmStatus !== 5) {
                        let rejectLabel = (apmt.apmStatus === 1) ? 'Từ chối' : 'Hủy lịch';
                        actionButtons += `<button class="button-reject" data-reject-id="${apmt.appointmentId}" data-apmt-status="${apmt.apmStatus}">${rejectLabel}</button>`;
                    }
                    if (apmt.apmStatus === 2 || apmt.apmStatus === 3) {
                        actionButtons += `<button class="button-complete" data-complete-id="${apmt.appointmentId}">Hoàn thành</button>`;
                    }
                }
                // Determine display status (virtual rejected)
                let displayStatus = getDisplayStatus(apmt);
                return `
                <tr data-apmt-id="${apmt.appointmentId}" data-apmt-date="${apmt.apmtDate}" data-apmt-time="${apmt.apmTime}" data-apmt-notes="${apmt.notes || ''}">
                    <td>${idx + 1}</td>
                    <td>${apmt.patientName}</td>
                    <td>${apmt.apmtDate}</td>
                    <td>${apmt.apmTime.slice(0,5)}</td>
                    <td>${apmt.notes || ''}</td>
                    <td class="status ${displayStatus.class}">${displayStatus.label}</td>
                    <td>${actionButtons}</td>
                </tr>
                `;
            }).join('')}
        </tbody>
    </table>
    <div id="appointment-message" style="margin-top:10px;"></div>`;
    section.innerHTML = html;

    // Accept button logic
    document.querySelectorAll('.button-accept').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-accept-id');
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=2`, {
                    method: 'PATCH',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error('Không thể chấp nhận lịch hẹn');
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                alert('Lỗi khi chấp nhận lịch hẹn.');
            }
        };
    });

    // Reject button logic
    document.querySelectorAll('.button-reject').forEach(btn => {
        btn.onclick = async function() {
            const apmtId = this.getAttribute('data-reject-id');
            const apmtStatus = parseInt(this.getAttribute('data-apmt-status'));
            let confirmMsg = (apmtStatus === 1) ? 'Bạn có chắc muốn từ chối lịch hẹn này không?' : 'Bạn có chắc muốn hủy lịch hẹn này không?';
            if (!confirm(confirmMsg)) return;
            this.disabled = true;
            setMessage('Đang xử lý...', false);
            try {
                // Always send status=4 (cancelled) to backend
                const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=4`, {
                    method: 'PATCH',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error('Không thể từ chối/hủy lịch hẹn');
                setMessage((apmtStatus === 1) ? 'Đã từ chối lịch hẹn!' : 'Đã hủy lịch hẹn!', true);
                // Mark _wasPending for frontend display if needed
                let appointments = await fetchAppointments();
                if (apmtStatus === 1) {
                  appointments = appointments.map(apmt => apmt.appointmentId == apmtId ? { ...apmt, _wasPending: true, apmStatus: 4 } : apmt);
                }
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Lỗi khi từ chối/hủy lịch hẹn.', false);
                this.disabled = false;
            }
        };
    });

    // Complete button logic
    document.querySelectorAll('.button-complete').forEach(btn => {
        btn.onclick = function() {
            const apmtId = this.getAttribute('data-complete-id');
            // Open modal and store appointment ID
            const completeModal = document.getElementById('completeAppointmentModal');
            const completeForm = document.getElementById('completeAppointmentForm');
            const completeNotes = document.getElementById('completeNotes');
            if (completeModal && completeForm && completeNotes) {
                completeModal.style.display = 'block';
                completeForm.setAttribute('data-apmt-id', apmtId);
                completeNotes.value = '';
            }
        };
    });
}

// Modal logic
window.addEventListener('DOMContentLoaded', async () => {
    let appointments = await fetchAppointments();
    console.log('Fetched appointments:', appointments); // Debug line
    renderAppointments(appointments);

    // Modal close/cancel (set up once)
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
    const completeAppointmentForm = document.getElementById('completeAppointmentForm');
    if (completeAppointmentForm) {
        completeAppointmentForm.onsubmit = async function(e) {
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
                if (!res.ok) throw new Error('Không thể hoàn thành lịch hẹn');
                setMessage('Lịch hẹn đã được đánh dấu hoàn thành!', true);
                closeCompleteModal();
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Lỗi khi hoàn thành lịch hẹn.', false);
                if (submitBtn) submitBtn.disabled = false;
            }
        };
    }

    // Only doctors can modify appointments
    if (userRoleName === 'doctor') {
        // Modify button logic
        document.querySelectorAll('.button-modify').forEach(btn => {
            btn.onclick = async function() {
                const apmtId = this.getAttribute('data-modify-id');
                const row = this.closest('tr');
                // Prefill modal
                document.getElementById('modifyDate').value = row.getAttribute('data-apmt-date');
                document.getElementById('modifyNotes').value = row.getAttribute('data-apmt-notes');
                document.getElementById('modifyAppointmentModal').style.display = 'block';
                document.getElementById('modifyAppointmentForm').setAttribute('data-apmt-id', apmtId);
                // Fetch available times for doctor
                const accId = localStorage.getItem('accId');
                let timeSelect = document.getElementById('modifyTime');
                timeSelect.innerHTML = '';
                try {
                    const wsRes = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${accId}`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    if (wsRes.ok) {
                        const schedules = await wsRes.json();
                        // Flatten all available times
                        let times = [];
                        schedules.forEach(sch => {
                            if (sch.startTime && sch.endTime) {
                                // Assume times are in HH:mm:ss format
                                let start = sch.startTime.slice(0,5);
                                let end = sch.endTime.slice(0,5);
                                // Add every 30 min slot between start and end
                                let [h1, m1] = start.split(':').map(Number);
                                let [h2, m2] = end.split(':').map(Number);
                                let t1 = h1 * 60 + m1;
                                let t2 = h2 * 60 + m2;
                                for (let t = t1; t <= t2; t += 30) {
                                    let h = Math.floor(t/60).toString().padStart(2,'0');
                                    let m = (t%60).toString().padStart(2,'0');
                                    times.push(`${h}:${m}`);
                                }
                            }
                        });
                        // Remove duplicates
                        times = [...new Set(times)];
                        times.forEach(time => {
                            let opt = document.createElement('option');
                            opt.value = time;
                            opt.textContent = time;
                            timeSelect.appendChild(opt);
                        });
                    }
                } catch (err) {
                    timeSelect.innerHTML = '<option value="">Không có thời gian khả dụng</option>';
                }
            };
        });

        // Modal close/cancel for modify
        document.getElementById('closeModifyModal').onclick = closeModifyModal;
        document.getElementById('cancelModifyBtn').onclick = closeModifyModal;
        function closeModifyModal() {
            document.getElementById('modifyAppointmentModal').style.display = 'none';
            document.getElementById('modifyAppointmentForm').reset();
            document.getElementById('modifyTime').innerHTML = '';
        }

        // Modal submit for modify
        document.getElementById('modifyAppointmentForm').onsubmit = async function(e) {
            e.preventDefault();
            const apmtId = this.getAttribute('data-apmt-id');
            const date = document.getElementById('modifyDate').value;
            let time = document.getElementById('modifyTime').value;
            const notes = document.getElementById('modifyNotes').value;
            // Validate date (not in the past)
            const today = new Date();
            const selDate = new Date(date);
            today.setHours(0,0,0,0);
            selDate.setHours(0,0,0,0);
            if (selDate < today) {
                alert('Ngày lịch hẹn không thể trong quá khứ.');
                return;
            }
            if (!time) {
                alert('Vui lòng chọn một thời gian hợp lệ.');
                return;
            }
            // Ensure time is in HH:mm:ss format
            if (time.length === 5) time += ':00';
            try {
                const res = await fetch(`https://localhost:7009/api/Appointment/UpdateAppointmentRequest?id=${apmtId}`, {
                    method: 'POST',
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
                if (!res.ok) throw new Error('Không thể cập nhật lịch hẹn');
                setMessage('Lịch hẹn đã được cập nhật thành công!', true);
                closeModifyModal();
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Lỗi khi cập nhật lịch hẹn.', false);
            }
        };
    }
});

function setMessage(msg, success) {
    const msgDiv = document.getElementById('appointment-message');
    msgDiv.textContent = msg;
    msgDiv.style.color = success ? '#27ae60' : '#c0392b';
}
// Get user role utilities
const userRoleId = window.roleUtils && window.roleUtils.getUserRole ? window.roleUtils.getUserRole() : null;
const userRoleName = (window.roleUtils && window.roleUtils.ROLE_NAMES && userRoleId) ? window.roleUtils.ROLE_NAMES[userRoleId] : 'guest';