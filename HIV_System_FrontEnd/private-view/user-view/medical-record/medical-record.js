// Render medical records with labeled options
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const recordList = document.getElementById('record-list');

    // Show loading state
    recordList.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Đang tải hồ sơ bệnh án của bạn...</p>
        </div>
    `;

    fetch("https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => {
        if (res.status === 404) {
            // Handle case where user has no medical records yet
            recordList.innerHTML = `
                <div class="no-records-container">
                    <div class="no-records-icon">
                        <i class="fas fa-clipboard-list"></i>
                    </div>
                    <h3>Không Tìm Thấy Hồ Sơ Bệnh Án</h3>
                    <p>Bạn chưa có hồ sơ bệnh án nào. Hồ sơ bệnh án sẽ xuất hiện ở đây sau khi bạn thăm khám với bác sĩ.</p>
                    <div class="suggestions">
                        <p><strong>Bạn có thể:</strong></p>
                        <ul>
                            <li>Đặt lịch hẹn với bác sĩ</li>
                            <li>Hoàn thành các cuộc hẹn đã lên lịch</li>
                            <li>Hồ sơ bệnh án sẽ được tạo sau khi thăm khám</li>
                        </ul>
                    </div>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Đặt Lịch Hẹn
                    </button>
                </div>
            `;
            return null;
        }
        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }
        return res.json();
    })
    .then(data => {
        if (!data) return; // Handle 404 case
        
        if (!data.appointments || !data.appointments.length) {
            recordList.innerHTML = `
                <div class="no-records-container">
                    <div class="no-records-icon">
                        <i class="fas fa-clipboard-list"></i>
                    </div>
                    <h3>Không Tìm Thấy Hồ Sơ Bệnh Án</h3>
                    <p>Bạn chưa có hồ sơ bệnh án nào. Hồ sơ bệnh án sẽ xuất hiện ở đây sau khi bạn thăm khám với bác sĩ.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Đặt Lịch Hẹn
                    </button>
                </div>
            `;
            return;
        }
        
        recordList.innerHTML = data.appointments.map(apm => `
            <div class="record-card">
                <div class="record-info">
                    <div><b>Bác sĩ:</b> ${apm.doctorName}</div>
                    <div><b>Ngày:</b> ${apm.apmtDate}</div>
                    <div><b>Giờ:</b> ${apm.apmTime}</div>
                    <div><b>Ghi chú:</b> ${apm.notes || "Không có ghi chú"}</div>
                </div>
                <div class="record-options">
                    <button class="record-option-btn" onclick="window.location.href='../appointment-view/view-appointment.html?id=${apm.appointmentId}'">Xem lịch hẹn</button>
                    <button class="record-option-btn" onclick="window.location.href='../test-result/test-result.html'">Xem kết quả xét nghiệm</button>
                    <button class="record-option-btn" onclick="window.location.href='../ARV/ARV.html'">Xem phác đồ ARV</button>
                </div>
            </div>
        `).join("");
    })
    .catch(err => {
        console.error('Error loading medical records:', err);
        recordList.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Lỗi Khi Tải Hồ Sơ Bệnh Án</h3>
                <p>Đã xảy ra lỗi khi tải hồ sơ bệnh án của bạn. Vui lòng thử lại sau.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Thử Lại
                </button>
            </div>
        `;
    });
});