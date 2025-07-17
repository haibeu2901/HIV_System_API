document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const medicationsGrid = document.getElementById('medicationsGrid');
    const loadingContainer = document.getElementById('loadingContainer');
    const searchInput = document.getElementById('searchInput');
    const filterButtons = document.querySelectorAll('.filter-btn');
    
    let allMedications = [];
    let filteredMedications = [];

    // Show loading state
    loadingContainer.style.display = 'flex';
    medicationsGrid.style.display = 'none';

    // Fetch all ARV medications
    fetch("https://localhost:7009/api/ArvMedicationDetail/GetAllArvMedicationDetails", {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
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
        loadingContainer.innerHTML = `                <div class="error-container">
                <div class="error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Lỗi tải danh sách thuốc</h3>
                <p>Đã xảy ra lỗi khi tải danh mục thuốc ARV. Vui lòng thử lại sau.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Thử lại
                </button>
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
                    <p>Không có thuốc nào phù hợp với tiêu chí tìm kiếm của bạn.</p>
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
                            <span><strong>Liều lượng:</strong> ${medication.arvMedicationDosage}</span>
                        </div>
                        
                        <div class="detail-item">
                            <i class="fas fa-building"></i>
                            <span><strong>Nhà sản xuất:</strong> ${medication.arvMedicationManufacturer}</span>
                        </div>
                        
                        <div class="detail-item price-item">
                            <i class="fas fa-money-bill-wave"></i>
                            <span><strong>Giá:</strong> ${medication.arvMedicationPrice.toLocaleString()} VND</span>
                        </div>
                    </div>
                </div>
                
                <div class="medication-footer">
                    <button class="btn-view-details" onclick="viewMedicationDetails('${medication.arvMedicationName}')">
                        <i class="fas fa-info-circle"></i> Xem chi tiết
                    </button>
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

    // Update statistics
    function updateStatistics(medications) {
        const totalMedications = medications.length;
        const uniqueManufacturers = new Set(medications.map(m => m.arvMedicationManufacturer)).size;
        const averagePrice = medications.reduce((sum, m) => sum + m.arvMedicationPrice, 0) / totalMedications;

        document.getElementById('totalMedications').textContent = totalMedications;
        document.getElementById('totalManufacturers').textContent = uniqueManufacturers;
        document.getElementById('averagePrice').textContent = Math.round(averagePrice).toLocaleString();
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

    // View medication details (placeholder function)
    window.viewMedicationDetails = function(medicationName) {
        const medication = allMedications.find(m => m.arvMedicationName === medicationName);
        if (medication) {
            // Create a modal or detailed view
            alert(`Chi tiết thuốc:\n\nTên thuốc: ${medication.arvMedicationName}\nMô tả: ${medication.arvMedicationDescription}\nLiều lượng: ${medication.arvMedicationDosage}\nNhà sản xuất: ${medication.arvMedicationManufacturer}\nGiá: ${medication.arvMedicationPrice.toLocaleString()} VND`);
        }
    };
});
