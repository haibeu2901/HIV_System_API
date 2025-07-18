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
    async viewRecord(pmrId) {
        console.log('Viewing medical record:', pmrId);
        
        const token = this.authManager.getToken();
        
        try {
            // Show loading state
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Loading detailed medical record...', 'info');
            }
            
            const response = await fetch(`https://localhost:7009/api/PatientMedicalRecord/GetPatientMedicalRecordById/${pmrId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const recordData = await response.json();
                console.log('Detailed medical record data:', recordData);
                this.showRecordModal(recordData);
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading detailed medical record:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Error loading medical record details', 'error');
            } else {
                alert('Error loading medical record details. Please try again.');
            }
        }
    }

    // Show record details in modal
    showRecordModal(recordData) {
        // Create modal if it doesn't exist
        if (!document.getElementById('medical-record-modal')) {
            this.createRecordModal();
        }
        
        const modal = document.getElementById('medical-record-modal');
        const modalBody = document.getElementById('medical-record-modal-body');
        
        // Helper functions
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
        
        const formatCurrency = (amount) => {
            return new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND'
            }).format(amount);
        };
        
        const formatDate = (dateString) => {
            return new Date(dateString).toLocaleDateString('vi-VN');
        };
        
        const formatTime = (timeString) => {
            return new Date(`1970-01-01T${timeString}`).toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit'
            });
        };
        
        // Build modal content
        modalBody.innerHTML = `
            <div class="modal-record-header">
                <h3>Medical Record Details</h3>
                <div class="record-info">
                    <span class="record-id">Medical Record ID: ${recordData.pmrId}</span>
                    <span class="patient-id">Patient ID: ${recordData.ptnId}</span>
                </div>
            </div>
            
            <div class="modal-sections">
                <!-- Appointments Section -->
                <div class="modal-section">
                    <div class="section-header">
                        <h4><i class="fas fa-calendar-check"></i> Appointments (${recordData.appointments?.length || 0})</h4>
                    </div>
                    <div class="section-content">
                        ${recordData.appointments?.length > 0 ? `
                            <div class="appointments-detailed">
                                ${recordData.appointments.map(apt => `
                                    <div class="appointment-detail-card">
                                        <div class="appointment-header">
                                            <div class="appointment-main-info">
                                                <h5>Appointment #${apt.appointmentId}</h5>
                                                <div class="appointment-datetime">
                                                    <span class="date"><i class="fas fa-calendar"></i> ${formatDate(apt.apmtDate)}</span>
                                                    <span class="time"><i class="fas fa-clock"></i> ${formatTime(apt.apmTime)}</span>
                                                </div>
                                            </div>
                                            <span class="status-badge status-${getAppointmentStatusClass(apt.apmStatus)}">${getAppointmentStatusText(apt.apmStatus)}</span>
                                        </div>
                                        <div class="appointment-details">
                                            <div class="doctor-info">
                                                <i class="fas fa-user-md"></i>
                                                <span><strong>Doctor:</strong> ${apt.doctorName}</span>
                                            </div>
                                            <div class="patient-info">
                                                <i class="fas fa-user"></i>
                                                <span><strong>Patient:</strong> ${apt.patientName}</span>
                                            </div>
                                            ${apt.notes ? `
                                                <div class="appointment-notes">
                                                    <i class="fas fa-sticky-note"></i>
                                                    <span><strong>Notes:</strong> ${apt.notes}</span>
                                                </div>
                                            ` : ''}
                                        </div>
                                    </div>
                                `).join('')}
                            </div>
                        ` : '<div class="no-data"><i class="fas fa-calendar-times"></i> No appointments found</div>'}
                    </div>
                </div>
                
                <!-- Test Results Section -->
                <div class="modal-section">
                    <div class="section-header">
                        <h4><i class="fas fa-vial"></i> Test Results (${recordData.testResults?.length || 0})</h4>
                    </div>
                    <div class="section-content">
                        ${recordData.testResults?.length > 0 ? `
                            <div class="test-results-detailed">
                                ${recordData.testResults.map(test => `
                                    <div class="test-result-detail-card">
                                        <div class="test-result-header">
                                            <div class="test-info">
                                                <h5>Test Result #${test.testResultId}</h5>
                                                <span class="test-date"><i class="fas fa-calendar"></i> ${formatDate(test.testDate)}</span>
                                            </div>
                                            <span class="test-result-badge ${test.result ? 'positive' : 'negative'}">${test.result ? 'Positive' : 'Negative'}</span>
                                        </div>
                                        <div class="test-result-content">
                                            ${test.notes ? `
                                                <div class="test-notes">
                                                    <i class="fas fa-sticky-note"></i>
                                                    <span><strong>Notes:</strong> ${test.notes}</span>
                                                </div>
                                            ` : ''}
                                            ${test.componentTestResults?.length > 0 ? `
                                                <div class="component-results-detailed">
                                                    <h6><i class="fas fa-list"></i> Component Test Results:</h6>
                                                    <div class="components-grid">
                                                        ${test.componentTestResults.map(comp => `
                                                            <div class="component-card">
                                                                <div class="component-header">
                                                                    <h6>${comp.componentTestResultName}</h6>
                                                                    <span class="component-value">${comp.resultValue}</span>
                                                                </div>
                                                                <p class="component-description">${comp.ctrDescription}</p>
                                                                ${comp.notes ? `<p class="component-notes"><strong>Notes:</strong> ${comp.notes}</p>` : ''}
                                                            </div>
                                                        `).join('')}
                                                    </div>
                                                </div>
                                            ` : ''}
                                        </div>
                                    </div>
                                `).join('')}
                            </div>
                        ` : '<div class="no-data"><i class="fas fa-flask"></i> No test results found</div>'}
                    </div>
                </div>
                
                <!-- ARV Regimens Section -->
                <div class="modal-section">
                    <div class="section-header">
                        <h4><i class="fas fa-prescription-bottle"></i> ARV Regimens (${recordData.arvRegimens?.length || 0})</h4>
                    </div>
                    <div class="section-content">
                        ${recordData.arvRegimens?.length > 0 ? `
                            <div class="arv-regimens-detailed">
                                ${recordData.arvRegimens.map(regimen => `
                                    <div class="arv-regimen-detail-card">
                                        <div class="regimen-header">
                                            <div class="regimen-info">
                                                <h5>ARV Regimen #${regimen.patientArvRegiId}</h5>
                                                <div class="regimen-meta">
                                                    <span class="regimen-level">Level ${regimen.regimenLevel}</span>
                                                    <span class="regimen-dates">
                                                        <i class="fas fa-calendar"></i>
                                                        ${formatDate(regimen.startDate)} - ${regimen.endDate ? formatDate(regimen.endDate) : 'Ongoing'}
                                                    </span>
                                                </div>
                                            </div>
                                            <span class="regimen-status status-${getRegimenStatusClass(regimen.regimenStatus)}">${getRegimenStatusText(regimen.regimenStatus)}</span>
                                        </div>
                                        <div class="regimen-content">
                                            <div class="regimen-summary">
                                                <div class="regimen-cost">
                                                    <i class="fas fa-money-bill-wave"></i>
                                                    <span><strong>Total Cost:</strong> ${formatCurrency(regimen.totalCost)}</span>
                                                </div>
                                                <div class="regimen-created">
                                                    <i class="fas fa-calendar-plus"></i>
                                                    <span><strong>Created:</strong> ${formatDate(regimen.createdAt)}</span>
                                                </div>
                                            </div>
                                            ${regimen.notes ? `
                                                <div class="regimen-notes">
                                                    <i class="fas fa-sticky-note"></i>
                                                    <span><strong>Notes:</strong> ${regimen.notes}</span>
                                                </div>
                                            ` : ''}
                                            ${regimen.arvMedications?.length > 0 ? `
                                                <div class="medications-detailed">
                                                    <h6><i class="fas fa-pills"></i> Medications:</h6>
                                                    <div class="medications-grid">
                                                        ${regimen.arvMedications.map(med => `
                                                            <div class="medication-card">
                                                                <div class="medication-header">
                                                                    <h6>${med.medicationDetail.arvMedicationName}</h6>
                                                                    <div class="medication-dosage">${med.medicationDetail.arvMedicationDosage}</div>
                                                                </div>
                                                                <div class="medication-details">
                                                                    <p class="medication-description">${med.medicationDetail.arvMedicationDescription}</p>
                                                                    <div class="medication-info">
                                                                        <span class="quantity"><i class="fas fa-box"></i> Quantity: ${med.quantity}</span>
                                                                        <span class="price"><i class="fas fa-tag"></i> Price: ${formatCurrency(med.medicationDetail.arvMedicationPrice)}</span>
                                                                    </div>
                                                                    <div class="manufacturer">
                                                                        <i class="fas fa-building"></i>
                                                                        <span><strong>Manufacturer:</strong> ${med.medicationDetail.arvMedicationManufacturer}</span>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        `).join('')}
                                                    </div>
                                                </div>
                                            ` : ''}
                                        </div>
                                    </div>
                                `).join('')}
                            </div>
                        ` : '<div class="no-data"><i class="fas fa-pills"></i> No ARV regimens found</div>'}
                    </div>
                </div>
            </div>
        `;
        
        // Show modal
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
    }

    // Create modal HTML structure
    createRecordModal() {
        const modalHTML = `
            <div id="medical-record-modal" class="modal-overlay">
                <div class="modal-container">
                    <div class="modal-header">
                        <h3>Medical Record Details</h3>
                        <button class="modal-close" onclick="medicalRecordManager.closeRecordModal()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div id="medical-record-modal-body">
                            <!-- Content will be loaded here -->
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        
        // Add modal styles
        this.addModalStyles();
    }

    // Close modal
    closeRecordModal() {
        const modal = document.getElementById('medical-record-modal');
        if (modal) {
            modal.style.display = 'none';
            document.body.classList.remove('modal-open');
        }
    }

    // Add modal styles
    addModalStyles() {
        if (document.getElementById('medical-record-modal-styles')) return;
        
        const styles = `
            <style id="medical-record-modal-styles">
                .modal-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: rgba(0, 0, 0, 0.5);
                    display: none;
                    z-index: 1000;
                    overflow-y: auto;
                }
                
                .modal-container {
                    background: white;
                    max-width: 1200px;
                    margin: 20px auto;
                    border-radius: 8px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                    position: relative;
                }
                
                .modal-header {
                    background: #f8f9fa;
                    padding: 20px;
                    border-bottom: 1px solid #e9ecef;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    border-radius: 8px 8px 0 0;
                }
                
                .modal-header h3 {
                    margin: 0;
                    color: #333;
                }
                
                .modal-close {
                    background: none;
                    border: none;
                    font-size: 24px;
                    cursor: pointer;
                    color: #666;
                    padding: 0;
                    width: 30px;
                    height: 30px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                
                .modal-close:hover {
                    color: #000;
                }
                
                .modal-body {
                    padding: 20px;
                    max-height: 80vh;
                    overflow-y: auto;
                }
                
                .modal-record-header {
                    margin-bottom: 20px;
                    padding-bottom: 15px;
                    border-bottom: 2px solid #e9ecef;
                }
                
                .modal-record-header h3 {
                    margin: 0 0 10px 0;
                    color: #333;
                }
                
                .record-info {
                    display: flex;
                    gap: 20px;
                    flex-wrap: wrap;
                }
                
                .record-id, .patient-id {
                    background: #e3f2fd;
                    color: #1976d2;
                    padding: 5px 10px;
                    border-radius: 4px;
                    font-size: 14px;
                    font-weight: 500;
                }
                
                .modal-section {
                    margin-bottom: 30px;
                }
                
                .section-header {
                    margin-bottom: 15px;
                    padding-bottom: 10px;
                    border-bottom: 1px solid #e9ecef;
                }
                
                .section-header h4 {
                    margin: 0;
                    color: #333;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                }
                
                .section-header i {
                    color: #007bff;
                }
                
                .appointment-detail-card, .test-result-detail-card, .arv-regimen-detail-card {
                    background: #f8f9fa;
                    border: 1px solid #e9ecef;
                    border-radius: 8px;
                    padding: 15px;
                    margin-bottom: 15px;
                }
                
                .appointment-header, .test-result-header, .regimen-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: flex-start;
                    margin-bottom: 10px;
                }
                
                .appointment-main-info h5, .test-info h5, .regimen-info h5 {
                    margin: 0 0 5px 0;
                    color: #333;
                }
                
                .appointment-datetime, .regimen-meta {
                    display: flex;
                    gap: 15px;
                    font-size: 14px;
                    color: #666;
                }
                
                .appointment-details, .test-result-content, .regimen-content {
                    margin-top: 10px;
                }
                
                .appointment-details > div, .test-result-content > div, .regimen-content > div {
                    margin-bottom: 8px;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    font-size: 14px;
                }
                
                .components-grid, .medications-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
                    gap: 15px;
                    margin-top: 10px;
                }
                
                .component-card, .medication-card {
                    background: white;
                    border: 1px solid #e9ecef;
                    border-radius: 6px;
                    padding: 12px;
                }
                
                .component-header, .medication-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 8px;
                }
                
                .component-header h6, .medication-header h6 {
                    margin: 0;
                    color: #333;
                    font-size: 14px;
                }
                
                .component-value, .medication-dosage {
                    background: #e8f5e8;
                    color: #2e7d32;
                    padding: 2px 8px;
                    border-radius: 4px;
                    font-size: 12px;
                    font-weight: 500;
                }
                
                .component-description, .medication-description {
                    font-size: 13px;
                    color: #666;
                    margin: 5px 0;
                }
                
                .medication-info {
                    display: flex;
                    gap: 15px;
                    margin: 8px 0;
                    font-size: 13px;
                }
                
                .manufacturer {
                    font-size: 13px;
                    color: #666;
                    margin-top: 5px;
                }
                
                .regimen-summary {
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 15px;
                    margin-bottom: 10px;
                    font-size: 14px;
                }
                
                .no-data {
                    text-align: center;
                    padding: 40px;
                    color: #666;
                }
                
                .no-data i {
                    font-size: 48px;
                    color: #ddd;
                    margin-bottom: 10px;
                }
                
                body.modal-open {
                    overflow: hidden;
                }
                
                @media (max-width: 768px) {
                    .modal-container {
                        margin: 10px;
                        max-width: calc(100% - 20px);
                    }
                    
                    .record-info {
                        flex-direction: column;
                        gap: 10px;
                    }
                    
                    .appointment-header, .test-result-header, .regimen-header {
                        flex-direction: column;
                        align-items: flex-start;
                        gap: 10px;
                    }
                    
                    .components-grid, .medications-grid {
                        grid-template-columns: 1fr;
                    }
                    
                    .regimen-summary {
                        grid-template-columns: 1fr;
                    }
                }
            </style>
        `;
        
        document.head.insertAdjacentHTML('beforeend', styles);
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
