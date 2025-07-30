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

// Appointment status mapping (add virtual status for frontend display)
const appointmentStatusMap = {
  1: "Chờ xác nhận", 
  2: "Đã lên lịch",  
  4: "Đã hủy",
  5: "Đã hoàn thành",
  6: "Đã từ chối"    
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

// Event delegation for view button
section.addEventListener('click', async function(e) {
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
    if (window.isStaff || window.isDoctor) {
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
    actions.querySelector('.button-accept')?.addEventListener('click', async function() {
        const apmtId = this.getAttribute('data-accept-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=2`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã được chấp nhận!', true);
            modal.style.display = 'none';
            let appointments = await fetchAppointments();
            renderAppointments(appointments);
        } catch (err) {
            let msg = err.message;
            // Try to extract error message if it's a JSON string with an 'error' property
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) {}
            setMessage('Lỗi khi chấp nhận lịch hẹn: ' + msg, false);
        }
    });
    // Reject button
    actions.querySelector('.button-reject')?.addEventListener('click', async function() {
        const apmtId = this.getAttribute('data-reject-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=4`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã bị từ chối!', true);
            modal.style.display = 'none';
            let appointments = await fetchAppointments();
            renderAppointments(appointments);
        } catch (err) {
            let msg = err.message;
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) {}
            setMessage('Lỗi khi từ chối lịch hẹn: ' + msg, false);
        }
    });
    // Cancel button
    actions.querySelector('.button-cancel')?.addEventListener('click', async function() {
        const apmtId = this.getAttribute('data-cancel-id');
        try {
            const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?appointmentId=${apmtId}&status=4`, {
                method: 'PATCH',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            setMessage('Lịch hẹn đã được hủy!', true);
            modal.style.display = 'none';
            let appointments = await fetchAppointments();
            renderAppointments(appointments);
        } catch (err) {
            let msg = err.message;
            try {
                const parsed = JSON.parse(msg);
                if (parsed && parsed.error) msg = parsed.error;
            } catch (e) {}
            setMessage('Lỗi khi hủy lịch hẹn: ' + msg, false);
        }
    });
    // Complete button
    actions.querySelector('.button-complete')?.addEventListener('click', function() {
        const apmtId = this.getAttribute('data-complete-id');
        // Open complete modal and set appointment ID
        const completeModal = document.getElementById('completeAppointmentModal');
        const completeForm = document.getElementById('completeAppointmentForm');
        if (completeForm) completeForm.setAttribute('data-apmt-id', apmtId);
        if (completeModal) completeModal.style.display = 'block';
        // Hide the appointment details modal so the complete modal is upfront
        modal.style.display = 'none';
    });

    // Add event listener for modify button (only in modal)
    actions.querySelector('.button-modify')?.addEventListener('click', function() {
        // Prefill and open modify modal
        document.getElementById('modifyDate').value = apmt.apmtDate || apmt.requestDate || '';
        document.getElementById('modifyNotes').value = apmt.notes || '';
        document.getElementById('modifyAppointmentModal').style.display = 'block';
        document.getElementById('modifyAppointmentForm').setAttribute('data-apmt-id', apmt.appointmentId);
        // Hide the appointment details modal so the modify modal is upfront
        modal.style.display = 'none';
        // Fetch and populate available time slots from work schedule
        const doctorId = apmt.doctorId;
        const date = apmt.apmtDate || apmt.requestDate || '';
        const timeSelect = document.getElementById('modifyTime');
        timeSelect.innerHTML = '<option>Đang tải...</option>';
        fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetPersonalWorkSchedules?doctorId=${doctorId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
        .then(res => res.json())
        .then(schedules => {
            // Filter for the selected date and available slots
            const slots = (schedules || []).filter(s => s.workDate === date && s.isAvailable);
            let options = [];
            slots.forEach(s => {
                let start = s.startTime.slice(0,5);
                let end = s.endTime.slice(0,5);
                let [sh, sm] = start.split(':').map(Number);
                let [eh, em] = end.split(':').map(Number);
                let current = new Date(0,0,0,sh,sm);
                let endTime = new Date(0,0,0,eh,em);
                while (current < endTime) {
                    let h = current.getHours().toString().padStart(2,'0');
                    let m = current.getMinutes().toString().padStart(2,'0');
                    options.push(`${h}:${m}`);
                    current.setHours(current.getHours() + 1);
                }
            });
            // Remove duplicates and sort
            options = Array.from(new Set(options)).sort();
            if (options.length > 0) {
                timeSelect.innerHTML = options.map(t => `<option value="${t}">${t}</option>`).join('');
                // Pre-select current time if present
                const currentTime = (apmt.apmTime || apmt.requestTime || '').slice(0,5);
                if (options.includes(currentTime)) {
                    timeSelect.value = currentTime;
                }
            } else {
                timeSelect.innerHTML = '<option value="">Không có khung giờ khả dụng</option>';
            }
        })
        .catch(() => {
            timeSelect.innerHTML = '<option value="">Lỗi tải khung giờ</option>';
        });
    });
}

document.getElementById('closeAppointmentDetailsModal').onclick = function() {
    document.getElementById('appointmentDetailsModal').style.display = 'none';
};

// Modal logic
window.addEventListener('DOMContentLoaded', async () => {
    let appointments = await fetchAppointments();
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
                        } catch (e2) {}
                    }
                    setMessage(msg, false);
                    if (submitBtn) submitBtn.disabled = false;
                    return;
                }
                setMessage('Lịch hẹn đã được đánh dấu hoàn thành!', true);
                // Close both modals
                document.getElementById('completeAppointmentModal').style.display = 'none';
                document.getElementById('appointmentDetailsModal').style.display = 'none';
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Lỗi khi hoàn thành lịch hẹn.', false);
                if (submitBtn) submitBtn.disabled = false;
            }
        };
    }

    // Only doctors can modify appointments
    if (window.isDoctor) { // Changed from userRoleName to window.isDoctor
        // Remove the duplicate per-button .button-modify event handler
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
                        // Try to read as plain text first (409 returns a string)
                        const text = await res.text();
                        if (text && text.trim().length > 0) {
                            msg = text;
                        } else {
                            // If not text, try to parse as JSON
                            const data = JSON.parse(text);
                            if (data) {
                                if (data.error) msg = data.error;
                                else if (data.message) msg = data.message;
                                else if (data.title) msg = data.title;
                                else if (data.detail) msg = data.detail;
                                else if (data.errors && typeof data.errors === 'object') {
                                    // Show the first error message in errors object
                                    const firstKey = Object.keys(data.errors)[0];
                                    if (firstKey && Array.isArray(data.errors[firstKey]) && data.errors[firstKey][0]) {
                                        msg = data.errors[firstKey][0];
                                    }
                                }
                            }
                        }
                    } catch (e) {
                        // If text or JSON parsing fails, fallback to default
                    }
                    setMessage(msg, false);
                    return;
                }
                setMessage('Lịch hẹn đã được cập nhật thành công!', true);
                // Close both modals
                document.getElementById('modifyAppointmentModal').style.display = 'none';
                document.getElementById('appointmentDetailsModal').style.display = 'none';
                let appointments = await fetchAppointments();
                renderAppointments(appointments);
            } catch (err) {
                setMessage('Lỗi khi cập nhật lịch hẹn.', false);
            }
        };
    }
});

// Replace setMessage to use popup modal
function setMessage(msg, success) {
    const modal = document.getElementById('messageModal');
    const modalText = document.getElementById('messageModalText');
    if (!modal || !modalText) return;
    modalText.textContent = msg;
    modalText.className = success ? 'modal-success' : 'modal-error';
    modal.classList.add('show');
}

// Modal close logic
window.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById('messageModal');
    const closeBtn = document.getElementById('closeMessageModal');
    if (closeBtn && modal) {
        closeBtn.onclick = function() {
            modal.classList.remove('show');
        };
        // Close when clicking outside modal-content
        window.onclick = function(event) {
            if (event.target === modal) {
                modal.classList.remove('show');
            }
        };
    }
});
// Get user role utilities
const userRoleId = window.roleUtils && window.roleUtils.getUserRole ? window.roleUtils.getUserRole() : null;
const userRoleName = (window.roleUtils && window.roleUtils.ROLE_NAMES && userRoleId) ? window.roleUtils.ROLE_NAMES[userRoleId] : 'guest';

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
        return `${y}-${m.padStart(2,'0')}-${d.padStart(2,'0')}`;
    }
    return '';
}