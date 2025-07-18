// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping (with virtual rejected for frontend)
const appointmentStatusMap = {
  1: "Chờ xác nhận", // Pending
  2: "Đã xác nhận",   // Scheduled
  3: "Đã xác nhận lại", // Rescheduled
  4: "Đã hủy",        // Cancelled
  5: "Đã hoàn thành", // Completed
  6: "Đã từ chối"     // Rejected (virtual, frontend only)
};

// Helper to determine display status (virtual rejected for pending->cancelled)
function getDisplayStatus(apmt) {
  if (apmt.apmStatus === 4 && apmt._wasPending) {
    return { label: appointmentStatusMap[6], class: 'status-6' };
  }
  return { label: appointmentStatusMap[apmt.apmStatus] || 'Không rõ', class: `status-${apmt.apmStatus}` };
}

async function fetchAppointments() {
  try {
    const response = await fetch('https://localhost:7009/api/Appointment/GetAllAppointments', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) throw new Error('Không thể tải danh sách lịch hẹn.');
    return await response.json();
  } catch (error) {
    console.error('Lỗi khi tải lịch hẹn:', error);
    return [];
  }
}

function renderAppointments(appointments) {
  const section = document.getElementById('staffAppointmentListSection');
  if (!appointments.length) {
    section.innerHTML = '<p>Không có lịch hẹn nào.</p>';
    return;
  }
  let html = `<table class="staff-appointment-table">
    <thead>
      <tr>
        <th>STT</th>
        <th>Tên bệnh nhân</th>
        <th>Tên bác sĩ</th>
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
        if (apmt.apmStatus === 1) {
          actionButtons += `<button class="button-accept" data-accept-id="${apmt.appointmentId}">Chấp nhận</button>`;
        }
        if (apmt.apmStatus !== 4 && apmt.apmStatus !== 5) {
          let rejectLabel = (apmt.apmStatus === 1) ? 'Từ chối' : 'Hủy lịch';
          actionButtons += `<button class="button-reject" data-reject-id="${apmt.appointmentId}" data-apmt-status="${apmt.apmStatus}">${rejectLabel}</button>`;
        }
        let displayStatus = getDisplayStatus(apmt);
        return `
          <tr data-apmt-id="${apmt.appointmentId}">
            <td>${idx + 1}</td>
            <td>${apmt.patientName}</td>
            <td>${apmt.doctorName}</td>
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
        if (!res.ok) throw new Error('Không thể chấp nhận lịch hẹn.');
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
      try {
        // Always send status=4 (cancelled) to backend
        const res = await fetch(`https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${apmtId}&status=4`, {
          method: 'PATCH',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Không thể từ chối/hủy lịch hẹn.');
        let appointments = await fetchAppointments();
        // Mark _wasPending for frontend display if needed
        if (apmtStatus === 1) {
          appointments = appointments.map(apmt => apmt.appointmentId == apmtId ? { ...apmt, _wasPending: true, apmStatus: 4 } : apmt);
        }
        renderAppointments(appointments);
      } catch (err) {
        alert('Lỗi khi từ chối/hủy lịch hẹn.');
        this.disabled = false;
      }
    };
  });
}

window.addEventListener('DOMContentLoaded', async () => {
  let appointments = await fetchAppointments();
  renderAppointments(appointments);
}); 