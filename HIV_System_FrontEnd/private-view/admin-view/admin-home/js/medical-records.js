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
        
        const recordsHTML = records.map(record => `
            <div class="medical-record-card">
                <div class="record-header">
                    <div class="patient-info">
                        <h3>Patient ID: ${record.ptnId}</h3>
                        <p class="record-id">Medical Record ID: ${record.mrId}</p>
                    </div>
                    <div class="record-actions">
                        <button class="btn-secondary" onclick="medicalRecordManager.viewRecord(${record.mrId})">
                            <i class="fas fa-eye"></i> View
                        </button>
                        <button class="btn-primary" onclick="medicalRecordManager.editRecord(${record.mrId})">
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
                                ${record.appointments.slice(0, 2).map(apt => `
                                    <div class="appointment-item">
                                        <span class="appointment-date">${new Date(apt.appointmentDate).toLocaleDateString()}</span>
                                        <span class="appointment-time">${apt.appointmentTime}</span>
                                        <span class="status-badge status-${apt.status?.toLowerCase() || 'pending'}">${apt.status || 'Pending'}</span>
                                    </div>
                                `).join('')}
                                ${record.appointments.length > 2 ? `<p class="more-items">+${record.appointments.length - 2} more appointments</p>` : ''}
                            </div>
                        </div>
                    ` : ''}
                    
                    ${record.testResults?.length > 0 ? `
                        <div class="record-section">
                            <h4><i class="fas fa-vial"></i> Recent Test Results</h4>
                            <div class="test-results-preview">
                                ${record.testResults.slice(0, 3).map(test => `
                                    <div class="test-result-item">
                                        <span class="test-name">${test.testName || 'Test'}</span>
                                        <span class="test-date">${new Date(test.testDate).toLocaleDateString()}</span>
                                        <span class="test-result">${test.result || 'Pending'}</span>
                                    </div>
                                `).join('')}
                                ${record.testResults.length > 3 ? `<p class="more-items">+${record.testResults.length - 3} more test results</p>` : ''}
                            </div>
                        </div>
                    ` : ''}
                    
                    ${record.arvRegimens?.length > 0 ? `
                        <div class="record-section">
                            <h4><i class="fas fa-prescription-bottle"></i> ARV Regimens</h4>
                            <div class="arv-regimens-preview">
                                ${record.arvRegimens.slice(0, 2).map(regimen => `
                                    <div class="arv-regimen-item">
                                        <span class="regimen-name">${regimen.regimenName || 'Regimen'}</span>
                                        <span class="regimen-status">${regimen.status || 'Active'}</span>
                                    </div>
                                `).join('')}
                                ${record.arvRegimens.length > 2 ? `<p class="more-items">+${record.arvRegimens.length - 2} more regimens</p>` : ''}
                            </div>
                        </div>
                    ` : ''}
                </div>
            </div>
        `).join('');
        
        recordsList.innerHTML = recordsHTML;
    }

    // View record details
    viewRecord(mrId) {
        console.log('Viewing record:', mrId);
        // Implementation for viewing record details
    }

    // Edit record
    editRecord(mrId) {
        console.log('Editing record:', mrId);
        // Implementation for editing record
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
