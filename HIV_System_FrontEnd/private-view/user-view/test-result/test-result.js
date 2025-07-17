document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const container = document.getElementById('result-container');

    // Show loading state
    container.innerHTML = `
        <div class="loading-container">
            <div class="loader"></div>
            <p>Đang tải kết quả xét nghiệm...</p>
        </div>
    `;

    fetch("https://localhost:7009/api/TestResult/personal", {
        headers: { 
            "Authorization": `Bearer ${token}`,
            "accept": "*/*"
        }
    })
    .then(res => {
        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }
        return res.json();
    })
    .then(data => {
        console.log('API Response:', data);
        
        if (!data || data.length === 0) {
            container.innerHTML = `
                <div class="no-results">
                    <div class="no-results-icon">
                        <i class="fa-solid fa-flask-vial"></i>
                    </div>
                    <h3>Không Tìm Thấy Kết Quả Xét Nghiệm</h3>
                    <p>Bạn chưa có kết quả xét nghiệm nào. Vui lòng liên hệ với bác sĩ để biết thêm thông tin.</p>
                </div>
            `;
            return;
        }

        // Display raw data from API response
        let resultsHTML = '<div class="test-results-raw">';
        
        data.forEach((testResult, index) => {
            resultsHTML += `
                <div class="test-result-item">
                    <h3>Kết Quả Xét Nghiệm ${index + 1}</h3>
                    <div class="raw-data">
                        <div class="data-field">
                            <span class="field-label">Mã Hồ Sơ Bệnh Án:</span>
                            <span class="field-value">${testResult.patientMedicalRecordId}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Kết quả:</span>
                            <span class="field-value">${testResult.result}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Ngày xét nghiệm:</span>
                            <span class="field-value">${testResult.testDate}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Ghi chú:</span>
                            <span class="field-value">${testResult.notes}</span>
                        </div>
                    </div>
                </div>
            `;
        });
        
        resultsHTML += '</div>';
        
        container.innerHTML = resultsHTML;
    })
    .catch(error => {
        console.error('Error loading test results:', error);
        container.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                </div>
                <h3>Lỗi Khi Tải Kết Quả Xét Nghiệm</h3>
                <p>Không thể tải kết quả xét nghiệm của bạn lúc này. Vui lòng thử lại sau hoặc liên hệ hỗ trợ nếu vấn đề vẫn tiếp tục.</p>
                <button onclick="location.reload()" class="retry-btn">
                    <i class="fa-solid fa-refresh"></i> Thử Lại
                </button>
            </div>
        `;
    });
});