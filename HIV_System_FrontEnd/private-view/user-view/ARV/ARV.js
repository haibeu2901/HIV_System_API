document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const arvList = document.getElementById('arv-list');

    // Show loading state
    arvList.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Loading your ARV regimens...</p>
        </div>
    `;

    fetch("https://localhost:7009/api/PatientArvRegimen/GetPersonalArvRegimens", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => {
        if (res.status === 404) {
            // Handle case where user has no ARV regimens yet
            arvList.innerHTML = `
                <div class="no-arv-container">
                    <div class="no-arv-icon">
                        <i class="fas fa-pills"></i>
                    </div>
                    <h3>No ARV Regimens Found</h3>
                    <p>You don't have any ARV regimens yet. ARV regimens will be prescribed by your doctor based on your medical condition and test results.</p>
                    <div class="arv-info">
                        <p><strong>What is ARV?</strong></p>
                        <p>Antiretroviral therapy (ART) is a treatment that uses HIV medicines to fight HIV infection. It helps people with HIV live longer, healthier lives and reduces the risk of HIV transmission.</p>
                    </div>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Book an Appointment
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
        
        if (!data || !data.length) {
            arvList.innerHTML = `
                <div class="no-arv-container">
                    <div class="no-arv-icon">
                        <i class="fas fa-pills"></i>
                    </div>
                    <h3>Không Tìm Thấy Phác Đồ ARV</h3>
                    <p>Bạn chưa có phác đồ ARV nào. Phác đồ ARV sẽ được bác sĩ kê đơn dựa trên tình trạng sức khỏe và kết quả xét nghiệm của bạn.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Đặt Lịch Hẹn
                    </button>
                </div>
            `;
            return;
        }
        
        arvList.innerHTML = data.map(reg => `
            <div class="arv-card">
                <div class="arv-header">
                    <div class="arv-id">
                        <i class="fas fa-id-card"></i>
                        <span>ID phác đồ: ${reg.patientArvRegiId}</span>
                    </div>
                    <div class="arv-status-badge">
                        <span class="arv-status ${renderStatusClass(reg.regimenStatus)}">${renderStatusText(reg.regimenStatus)}</span>
                    </div>
                </div>
                
                <div class="arv-notes">
                    <i class="fas fa-notes-medical"></i>
                    <strong>Ghi chú:</strong> ${reg.notes || 'Không có ghi chú'}
                </div>
                
                <div class="arv-meta">
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-start"></i>
                        <strong>Ngày bắt đầu:</strong> ${formatDate(reg.startDate)}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-end"></i>
                        <strong>Ngày kết thúc:</strong> ${reg.endDate ? formatDate(reg.endDate) : '<span class="ongoing">Đang tiếp tục</span>'}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-layer-group"></i>
                        <strong>Bậc:</strong> ${reg.regimenLevel}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-plus"></i>
                        <strong>Ngày tạo:</strong> ${formatDate(reg.createdAt)}
                    </div>
                </div>
                
                <div class="arv-cost">
                    <i class="fas fa-coins"></i>
                    <strong>Tổng chi phí:</strong> ${reg.totalCost ? reg.totalCost.toLocaleString() + " VND" : "Chưa xác định"}
                </div>
                
                <div class="arv-medications">
                    <h4><i class="fas fa-pills"></i> Thuốc (${reg.arvMedications?.length || 0})</h4>
                    <div class="medications-grid">
                        ${reg.arvMedications?.map(med => `
                            <div class="medication-card">
                                <div class="medication-header">
                                    <h5>${med.medicationDetail.arvMedicationName}</h5>
                                    <span class="medication-dosage">${med.medicationDetail.arvMedicationDosage}</span>
                                </div>
                                <div class="medication-info">
                                    <p class="medication-description">${med.medicationDetail.arvMedicationDescription}</p>
                                    <div class="medication-details">
                                        <div class="detail-item">
                                            <i class="fas fa-building"></i>
                                            <span><strong>Nhà sản xuất:</strong> ${med.medicationDetail.arvMedicationManufacturer}</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-hashtag"></i>
                                            <span><strong>Số lượng:</strong> ${med.quantity}</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-money-bill"></i>
                                            <span><strong>Đơn giá:</strong> ${med.medicationDetail.arvMedicationPrice?.toLocaleString()} VND</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-calculator"></i>
                                            <span><strong>Thành tiền:</strong> ${(med.medicationDetail.arvMedicationPrice * med.quantity).toLocaleString()} VND</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        `).join('') || '<p class="no-medications">Không có thuốc nào được liệt kê</p>'}
                    </div>
                </div>
            </div>
        `).join('');
    })
    .catch(err => {
        console.error('Error loading ARV regimens:', err);
        arvList.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Lỗi Khi Tải Phác Đồ ARV</h3>
                <p>Đã xảy ra lỗi khi tải phác đồ ARV của bạn. Vui lòng thử lại sau.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Thử Lại
                </button>
            </div>
        `;
    });

    function formatDate(dateString) {
        if (!dateString) return 'Chưa xác định';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-GB', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    function renderStatusText(status) {
        switch(status) {
            case 1: return "Đang sử dụng";
            case 2: return "Đã hoàn thành";
            case 3: return "Đã dừng";
            default: return `Trạng thái ${status}`;
        }
    }

    function renderStatusClass(status) {
        switch(status) {
            case 1: return "active";
            case 2: return "completed";
            case 3: return "stopped";
            default: return "unknown";
        }
    }
});