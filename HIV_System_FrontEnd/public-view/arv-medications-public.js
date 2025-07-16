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
                <h3>Information Temporarily Unavailable</h3>
                <p>We're currently unable to load the medication information. Please try again later or contact our clinic directly.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Try Again
                </button>
                <a href="/landingpage.html#contact" class="btn-contact">
                    <i class="fas fa-phone"></i> Contact Clinic
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
                    <h3>No Medications Found</h3>
                    <p>No medications match your current search criteria. Try adjusting your search terms.</p>
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
                            <span><strong>Standard Dosage:</strong> ${medication.arvMedicationDosage}</span>
                        </div>
                        
                        <div class="detail-item">
                            <i class="fas fa-building"></i>
                            <span><strong>Manufacturer:</strong> ${medication.arvMedicationManufacturer}</span>
                        </div>
                        
                        <div class="detail-item info-item">
                            <i class="fas fa-info-circle"></i>
                            <span><strong>Category:</strong> ${getMedicationFullCategory(medication.arvMedicationDescription)}</span>
                        </div>
                    </div>
                </div>
                
                <div class="medication-footer">
                    <button class="btn-learn-more" onclick="showMedicationInfo('${medication.arvMedicationName}')">
                        <i class="fas fa-book-open"></i> Learn More
                    </button>
                    <div class="disclaimer">
                        <i class="fas fa-exclamation-triangle"></i>
                        <small>Consult healthcare provider before use</small>
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
        if (description.includes('NRTI')) return 'Nucleoside Reverse Transcriptase Inhibitor';
        if (description.includes('NNRTI')) return 'Non-Nucleoside Reverse Transcriptase Inhibitor';
        if (description.includes('PI')) return 'Protease Inhibitor';
        if (description.includes('INSTI')) return 'Integrase Strand Transfer Inhibitor';
        return 'Other HIV Medication';
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
                                <strong>Name:</strong> ${medication.arvMedicationName}
                            </div>
                            <div class="info-item">
                                <strong>Category:</strong> ${category}
                            </div>
                            <div class="info-item">
                                <strong>Standard Dosage:</strong> ${medication.arvMedicationDosage}
                            </div>
                            <div class="info-item">
                                <strong>Manufacturer:</strong> ${medication.arvMedicationManufacturer}
                            </div>
                        </div>
                    </div>
                    
                    <div class="medication-description">
                        <h3>Description</h3>
                        <p>${medication.arvMedicationDescription}</p>
                    </div>
                    
                    <div class="important-notice">
                        <i class="fas fa-exclamation-triangle"></i>
                        <h3>Important Medical Notice</h3>
                        <ul>
                            <li>This information is for educational purposes only</li>
                            <li>Always consult with a healthcare provider before starting any medication</li>
                            <li>HIV treatment requires professional medical supervision</li>
                            <li>Medication dosage and suitability vary by individual</li>
                            <li>Side effects and drug interactions must be considered</li>
                        </ul>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn-close" onclick="closeModal(this.closest('.educational-modal'))">
                        <i class="fas fa-times"></i> Close
                    </button>
                    <a href="../landingpage.html#contact" class="btn-contact">
                        <i class="fas fa-phone"></i> Contact Our Clinic
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
