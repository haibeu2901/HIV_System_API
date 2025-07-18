document.addEventListener('DOMContentLoaded', () => {
    const medicationsGrid = document.getElementById('medicationsGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    const searchInput = document.getElementById('searchInput');
    const filterButtons = document.querySelectorAll('.filter-btn');
    
    let allMedications = [];
    let filteredMedications = [];

    // Show loading state
    loadingContainer.style.display = 'flex';
    medicationsGrid.style.display = 'none';

    // Fetch all ARV medications (public access - no authentication needed)
    fetch("https://localhost:7009/api/ArvMedicationDetail/GetAllArvMedicationDetails", {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        allMedications = data;
        filteredMedications = data;
        
        // Hide loading and show content
        loadingContainer.style.display = 'none';
        medicationsGrid.style.display = 'grid';
        
        // Update statistics
        updateStatistics(allMedications);
        
        // Display medications
        displayMedications(filteredMedications);
    })
    .catch(error => {
        console.error('Error fetching ARV medications:', error);
        loadingContainer.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Thông tin tạm thời không có sẵn</h3>
                <p>Hiện tại chúng tôi không thể tải thông tin thuốc. Vui lòng thử lại sau hoặc liên hệ trực tiếp với phòng khám của chúng tôi.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Thử lại
                </button>
                <a href="/landingpage.html#contact" class="btn-contact">
                    <i class="fas fa-phone"></i> Liên hệ Phòng khám
                </a>
            </div>
        `;
    });

    // Display medications in grid
    function displayMedications(medications) {
        if (!medications || medications.length === 0) {
            medicationsGrid.innerHTML = `
                <div class="no-medications">
                    <div class="no-medications-icon">
                        <i class="fas fa-pills"></i>
                    </div>
                    <h3>Không tìm thấy thuốc</h3>
                    <p>Không có loại thuốc nào phù hợp với tiêu chí tìm kiếm hiện tại của bạn. Hãy thử điều chỉnh thuật ngữ tìm kiếm.</p>
                </div>
            `;
            return;
        }

        medicationsGrid.innerHTML = medications.map(medication => `
            <div class="medication-card" data-category="${getMedicationCategory(medication.arvMedicationDescription)}">
                <div class="medication-header">
                    <h3 class="medication-name">${medication.arvMedicationName}</h3>
                    <div class="medication-category">${getMedicationCategory(medication.arvMedicationDescription)}</div>
                </div>
                
                <div class="medication-content">
                    <div class="medication-description">
                        <p>${medication.arvMedicationDescription}</p>
                    </div>
                    
                    <div class="medication-details">
                        <div class="detail-item">
                            <i class="fas fa-prescription-bottle"></i>
                            <span><strong>Liều dùng tiêu chuẩn:</strong> ${medication.arvMedicationDosage}</span>
                        </div>
                        
                        <div class="detail-item">
                            <i class="fas fa-building"></i>
                            <span><strong>Nhà sản xuất:</strong> ${medication.arvMedicationManufacturer}</span>
                        </div>
                        
                        <div class="detail-item info-item">
                            <i class="fas fa-info-circle"></i>
                            <span><strong>Loại:</strong> ${getMedicationFullCategory(medication.arvMedicationDescription)}</span>
                        </div>
                    </div>
                </div>
                
                <div class="medication-footer">
                    <button class="btn-learn-more" onclick="showMedicationInfo('${medication.arvMedicationName}')">
                        <i class="fas fa-book-open"></i> Tìm hiểu thêm
                    </button>
                    <div class="disclaimer">
                        <i class="fas fa-exclamation-triangle"></i>
                        <small>Tham khảo ý kiến bác sĩ trước khi sử dụng</small>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // Get medication category based on description
    function getMedicationCategory(description) {
        if (description.includes('NRTI')) return 'NRTI';
        if (description.includes('NNRTI')) return 'NNRTI';
        if (description.includes('PI')) return 'PI';
        if (description.includes('INSTI')) return 'INSTI';
        return 'Other';
    }

    // Get full medication category name
    function getMedicationFullCategory(description) {
        if (description.includes('NRTI')) return 'Chất ức chế phiên mã ngược nucleoside';
        if (description.includes('NNRTI')) return 'Chất ức chế phiên mã ngược không phải nucleoside';
        if (description.includes('PI')) return 'Chất ức chế protease';
        if (description.includes('INSTI')) return 'Chất ức chế chuyển sợi Integrase';
        return 'Thuốc điều trị HIV khác';
    }

    // Update statistics
    function updateStatistics(medications) {
        const totalMedications = medications.length;
        const uniqueManufacturers = new Set(medications.map(m => m.arvMedicationManufacturer)).size;

        document.getElementById('totalMedications').textContent = totalMedications;
        document.getElementById('totalManufacturers').textContent = uniqueManufacturers;
    }

    // Search functionality
    searchInput.addEventListener('input', (e) => {
        const searchTerm = e.target.value.toLowerCase();
        filteredMedications = allMedications.filter(medication => 
            medication.arvMedicationName.toLowerCase().includes(searchTerm) ||
            medication.arvMedicationDescription.toLowerCase().includes(searchTerm) ||
            medication.arvMedicationManufacturer.toLowerCase().includes(searchTerm)
        );
        displayMedications(filteredMedications);
    });

    // Filter functionality
    filterButtons.forEach(button => {
        button.addEventListener('click', () => {
            // Update active button
            filterButtons.forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');
            
            const filter = button.getAttribute('data-filter');
            
            if (filter === 'all') {
                filteredMedications = allMedications;
            } else {
                filteredMedications = allMedications.filter(medication => {
                    const category = getMedicationCategory(medication.arvMedicationDescription);
                    return category.toLowerCase() === filter.toLowerCase();
                });
            }
            
            displayMedications(filteredMedications);
        });
    });

    // Show medication information (educational modal)
    window.showMedicationInfo = function(medicationName) {
        const medication = allMedications.find(m => m.arvMedicationName === medicationName);
        if (medication) {
            const category = getMedicationFullCategory(medication.arvMedicationDescription);
            const modal = createEducationalModal(medication, category);
            document.body.appendChild(modal);
        }
    };

    // Create educational modal
    function createEducationalModal(medication, category) {
        const modal = document.createElement('div');
        modal.className = 'educational-modal';
        modal.innerHTML = `
            <div class="modal-overlay" onclick="closeModal(this.parentElement)"></div>
            <div class="modal-content">
                <div class="modal-header">
                    <h2><i class="fas fa-pills"></i> ${medication.arvMedicationName}</h2>
                    <button class="modal-close" onclick="closeModal(this.closest('.educational-modal'))">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="medication-info">
                        <h3>Medication Information</h3>
                        <div class="info-grid">
                            <div class="info-item">
                                <strong>Tên:</strong> ${medication.arvMedicationName}
                            </div>
                            <div class="info-item">
                                <strong>Loại:</strong> ${category}
                            </div>
                            <div class="info-item">
                                <strong>Liều dùng tiêu chuẩn:</strong> ${medication.arvMedicationDosage}
                            </div>
                            <div class="info-item">
                                <strong>Nhà sản xuất:</strong> ${medication.arvMedicationManufacturer}
                            </div>
                        </div>
                    </div>
                    
                    <div class="medication-description">
                        <h3>Mô tả</h3>
                        <p>${medication.arvMedicationDescription}</p>
                    </div>
                    
                    <div class="important-notice">
                        <i class="fas fa-exclamation-triangle"></i>
                        <h3>Thông báo y tế quan trọng</h3>
                        <ul>
                            <li>Thông tin này chỉ dành cho mục đích giáo dục</li>
                            <li>Luôn luôn tham khảo ý kiến của nhà cung cấp dịch vụ chăm sóc sức khỏe trước khi bắt đầu dùng bất kỳ loại thuốc nào</li>
                            <li>Điều trị HIV cần có sự giám sát y tế chuyên nghiệp</li>
                            <li>Liều lượng và tính phù hợp của thuốc thay đổi tùy theo từng cá nhân</li>
                            <li>Tác dụng phụ và tương tác thuốc phải được xem xét</li>
                        </ul>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn-close" onclick="closeModal(this.closest('.educational-modal'))">
                        <i class="fas fa-times"></i> Đóng
                    </button>
                    <a href="../landingpage.html#contact" class="btn-contact">
                        <i class="fas fa-phone"></i> Liên hệ với phòng khám của chúng tôi
                    </a>
                </div>
            </div>
        `;
        return modal;
    }

    // Close modal function
    window.closeModal = function(modal) {
        modal.remove();
    };
});
