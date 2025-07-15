// Medical Records Module
class MedicalRecordManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load medical records
    async loadMedicalRecords() {
        const recordsList = document.getElementById('medical-records-list');
        recordsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/PatientMedicalRecord/GetPatientsMedicalRecord', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                console.log('Medical records data:', data);
                
                if (!data || data.length === 0) {
                    recordsList.innerHTML = '<div class="no-data">No medical records found</div>';
                    return;
                }
                
                this.renderMedicalRecords(data);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading medical records:', error);
            recordsList.innerHTML = '<div class="error-message">Error loading medical records. Please try again.</div>';
        }
    }

    // Search medical records
    searchMedicalRecords() {
        const searchTerm = document.getElementById('medical-search').value;
        console.log('Searching medical records:', searchTerm);
        this.loadMedicalRecords();
    }

    // Render medical records
    renderMedicalRecords(records) {
        const recordsList = document.getElementById('medical-records-list');
        
        const recordsHTML = records.map(record => {
            // Get appointment status mapping
            const getAppointmentStatusText = (status) => {
                const statusMap = {
                    1: 'Scheduled',
                    2: 'Completed',
                    3: 'Pending',
                    4: 'Cancelled',
                    5: 'No Show'
                };
                return statusMap[status] || 'Unknown';
            };
            
            const getAppointmentStatusClass = (status) => {
                const classMap = {
                    1: 'scheduled',
                    2: 'completed',
                    3: 'pending',
                    4: 'cancelled',
                    5: 'no-show'
                };
                return classMap[status] || 'unknown';
            };
            
            // Get regimen status
            const getRegimenStatusText = (status) => {
                const statusMap = {
                    1: 'Initiated',
                    2: 'Active',
                    3: 'Completed',
                    4: 'Paused',
                    5: 'Discontinued'
                };
                return statusMap[status] || 'Unknown';
            };
            
            const getRegimenStatusClass = (status) => {
                const classMap = {
                    1: 'initiated',
                    2: 'active',
                    3: 'completed',
                    4: 'paused',
                    5: 'discontinued'
                };
                return classMap[status] || 'unknown';
            };
            
            // Format currency
            const formatCurrency = (amount) => {
                return new Intl.NumberFormat('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                }).format(amount);
            };
            
            return `
                <div class="medical-record-card">
                    <div class="record-header">
                        <div class="patient-info">
                            <h3>Patient ID: ${record.ptnId}</h3>
                            <p class="record-id">Medical Record ID: ${record.pmrId}</p>
                        </div>
                        <div class="record-actions">
                            <button class="btn-secondary" onclick="medicalRecordManager.viewRecord(${record.pmrId})">
                                <i class="fas fa-eye"></i> View Details
                            </button>
                            <button class="btn-primary" onclick="medicalRecordManager.editRecord(${record.pmrId})">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                        </div>
                    </div>
                    
                    <div class="record-content">
                        <div class="record-stats">
                            <div class="stat-item">
                                <i class="fas fa-calendar-alt"></i>
                                <span>Appointments: ${record.appointments?.length || 0}</span>
                            </div>
                            <div class="stat-item">
                                <i class="fas fa-flask"></i>
                                <span>Test Results: ${record.testResults?.length || 0}</span>
                            </div>
                            <div class="stat-item">
                                <i class="fas fa-pills"></i>
                                <span>ARV Regimens: ${record.arvRegimens?.length || 0}</span>
                            </div>
                        </div>
                        
                        ${record.appointments?.length > 0 ? `
                            <div class="record-section">
                                <h4><i class="fas fa-calendar-check"></i> Recent Appointments</h4>
                                <div class="appointments-preview">
                                    ${record.appointments.slice(0, 3).map(apt => `
                                        <div class="appointment-item">
                                            <div class="appointment-info">
                                                <span class="appointment-date">${new Date(apt.apmtDate).toLocaleDateString()}</span>
                                                <span class="appointment-time">${apt.apmTime}</span>
                                                <span class="doctor-name">Dr. ${apt.doctorName}</span>
                                            </div>
                                            <div class="appointment-status">
                                                <span class="status-badge status-${getAppointmentStatusClass(apt.apmStatus)}">${getAppointmentStatusText(apt.apmStatus)}</span>
                                            </div>
                                            ${apt.notes ? `<div class="appointment-notes">${apt.notes}</div>` : ''}
                                        </div>
                                    `).join('')}
                                    ${record.appointments.length > 3 ? `<p class="more-items">+${record.appointments.length - 3} more appointments</p>` : ''}
                                </div>
                            </div>
                        ` : '<div class="no-data-section"><i class="fas fa-calendar-times"></i> No appointments scheduled</div>'}
                        
                        ${record.testResults?.length > 0 ? `
                            <div class="record-section">
                                <h4><i class="fas fa-vial"></i> Test Results</h4>
                                <div class="test-results-preview">
                                    ${record.testResults.map(test => `
                                        <div class="test-result-item">
                                            <div class="test-header">
                                                <span class="test-date">${new Date(test.testDate).toLocaleDateString()}</span>
                                                <span class="test-result-badge ${test.result ? 'positive' : 'negative'}">${test.result ? 'Positive' : 'Negative'}</span>
                                            </div>
                                            <div class="test-notes">${test.notes || 'No notes'}</div>
                                            ${test.componentTestResults?.length > 0 ? `
                                                <div class="component-results">
                                                    <strong>Components:</strong>
                                                    ${test.componentTestResults.slice(0, 3).map(comp => `
                                                        <div class="component-item">
                                                            <span class="component-name">${comp.componentTestResultName}</span>
                                                            <span class="component-value">${comp.resultValue}</span>
                                                        </div>
                                                    `).join('')}
                                                    ${test.componentTestResults.length > 3 ? `<div class="more-components">+${test.componentTestResults.length - 3} more components</div>` : ''}
                                                </div>
                                            ` : ''}
                                        </div>
                                    `).join('')}
                                </div>
                            </div>
                        ` : '<div class="no-data-section"><i class="fas fa-flask"></i> No test results available</div>'}
                        
                        ${record.arvRegimens?.length > 0 ? `
                            <div class="record-section">
                                <h4><i class="fas fa-prescription-bottle"></i> ARV Regimens</h4>
                                <div class="arv-regimens-preview">
                                    ${record.arvRegimens.map(regimen => `
                                        <div class="arv-regimen-item">
                                            <div class="regimen-header">
                                                <div class="regimen-info">
                                                    <span class="regimen-level">Level ${regimen.regimenLevel}</span>
                                                    <span class="regimen-dates">${new Date(regimen.startDate).toLocaleDateString()} - ${regimen.endDate ? new Date(regimen.endDate).toLocaleDateString() : 'Ongoing'}</span>
                                                </div>
                                                <span class="regimen-status status-${getRegimenStatusClass(regimen.regimenStatus)}">${getRegimenStatusText(regimen.regimenStatus)}</span>
                                            </div>
                                            <div class="regimen-cost">Total Cost: ${formatCurrency(regimen.totalCost)}</div>
                                            <div class="regimen-notes">${regimen.notes || 'No notes'}</div>
                                            ${regimen.arvMedications?.length > 0 ? `
                                                <div class="medications-list">
                                                    <strong>Medications:</strong>
                                                    ${regimen.arvMedications.map(med => `
                                                        <div class="medication-item">
                                                            <span class="med-name">${med.medicationDetail.arvMedicationName}</span>
                                                            <span class="med-dosage">${med.medicationDetail.arvMedicationDosage}</span>
                                                            <span class="med-quantity">Qty: ${med.quantity}</span>
                                                        </div>
                                                    `).join('')}
                                                </div>
                                            ` : ''}
                                        </div>
                                    `).join('')}
                                </div>
                            </div>
                        ` : '<div class="no-data-section"><i class="fas fa-pills"></i> No ARV regimens prescribed</div>'}
                    </div>
                </div>
            `;
        }).join('');
        
        recordsList.innerHTML = recordsHTML;
    }

    // View record details
    viewRecord(pmrId) {
        console.log('Viewing medical record:', pmrId);
        
        if (window.utils && window.utils.showToast) {
            window.utils.showToast('Detailed view functionality coming soon!', 'info');
        } else {
            alert('Detailed view functionality coming soon!');
        }
        
        // Future implementation could show a modal with detailed information
        // about appointments, test results, and ARV regimens
    }

    // Edit record
    editRecord(pmrId) {
        console.log('Editing medical record:', pmrId);
        
        if (window.utils && window.utils.showToast) {
            window.utils.showToast('Edit functionality coming soon!', 'info');
        } else {
            alert('Edit functionality coming soon!');
        }
        
        // Future implementation could show an edit form for medical record details
    }

    // Initialize
    init() {
        // Search functionality
        const medicalSearchBtn = document.getElementById('medical-search-btn');
        if (medicalSearchBtn) {
            medicalSearchBtn.addEventListener('click', () => this.searchMedicalRecords());
        }
        
        const medicalSearchInput = document.getElementById('medical-search');
        if (medicalSearchInput) {
            medicalSearchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.searchMedicalRecords();
                }
            });
        }
    }
}

// Export for use in other modules
window.MedicalRecordManager = MedicalRecordManager;
