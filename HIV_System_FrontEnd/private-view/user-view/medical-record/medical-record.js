// Get token from localStorage
const token = localStorage.getItem('token');

// Appointment status mapping
const appointmentStatusMap = {
    1: 'Chờ xác nhận',
    2: 'Đã xác nhận',
    3: 'Đã xác nhận lại',
    4: 'Đã hủy',
    5: 'Đã hoàn thành'
};

// ARV Regimen level and status mapping
const regimenLevelMap = {
    1: "Level 1",
    2: "Level 2",
    3: "Level 3",
    4: "Special Case"
};
const regimenStatusMap = {
    1: "Planned",
    2: "Active",
    3: "Paused",
    4: "Failed",
    5: "Completed"
};

// Fetch patient medical record data
async function fetchPatientMedicalData() {
    try {
        const response = await fetch('https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            if (response.status === 404) {
                return null; // No medical records found
            }
            throw new Error('Failed to fetch medical data');
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching medical data:', error);
        return null;
    }
}

// Fetch ARV regimens for the current patient
async function fetchPatientArvRegimens() {
    try {
        const response = await fetch('https://localhost:7009/api/PatientArvRegimen/GetPersonalArvRegimens', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            if (response.status === 404) {
                return []; // No ARV regimens found
            }
            throw new Error('Failed to fetch ARV regimens');
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV regimens:', error);
        return [];
    }
}

// Render appointments
function renderAppointments(appointments) {
    const section = document.getElementById('appointmentsContent');
    
    if (!appointments || appointments.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-calendar-times"></i>
                <p>No appointments found.</p>
                <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                    <i class="fas fa-calendar-plus"></i> Book an Appointment
                </button>
            </div>
        `;
        return;
    }

    let html = `
        <div class="appointments-stats">
            <div class="stat-card">
                <div class="stat-number">${appointments.length}</div>
                <div class="stat-label">Total Appointments</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${appointments.filter(a => a.apmStatus === 5).length}</div>
                <div class="stat-label">Completed</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${appointments.filter(a => a.apmStatus === 2).length}</div>
                <div class="stat-label">Confirmed</div>
            </div>
        </div>
        <table class="appointments-table">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Doctor</th>
                    <th>Status</th>
                    <th>Notes</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
    `;

    appointments.forEach(appt => {
        const statusLabel = appointmentStatusMap[appt.apmStatus] || 'Unknown';
        const statusClass = `status-${appt.apmStatus}`;
        
        html += `
            <tr>
                <td>${appt.apmtDate}</td>
                <td>${appt.apmTime ? appt.apmTime.slice(0, 5) : '-'}</td>
                <td>${appt.doctorName || '-'}</td>
                <td><span class="appointment-status ${statusClass}">${statusLabel}</span></td>
                <td>${appt.notes || '-'}</td>
                <td>
                    <button class="action-btn" onclick="window.location.href='../appointment-view/view-appointment.html?id=${appt.appointmentId}'">
                        <i class="fas fa-eye"></i> View
                    </button>
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;
    
    section.innerHTML = html;
}

// Render test results
function renderTestResults(testResults) {
    const section = document.getElementById('testResultsContent');
    
    if (!testResults || testResults.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-flask"></i>
                <p>No test results found.</p>
                <p class="empty-state-subtitle">Test results will appear here after your laboratory tests.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="test-results-stats">
            <div class="stat-card">
                <div class="stat-number">${testResults.length}</div>
                <div class="stat-label">Total Tests</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${testResults.filter(t => t.result === true).length}</div>
                <div class="stat-label">Positive</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${testResults.filter(t => t.result === false).length}</div>
                <div class="stat-label">Negative</div>
            </div>
        </div>
        <div class="test-results-grid">
    `;
    
    testResults.forEach(testResult => {
        const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
        const resultText = testResult.result ? 'Positive' : 'Negative';

        html += `
            <div class="test-result-card">
                <div class="test-result-header">
                    <span class="test-result-date">Test Date: ${testResult.testDate}</span>
                    <span class="test-result-overall ${resultClass}">${resultText}</span>
                </div>
        `;

        if (testResult.notes) {
            html += `
                <div class="test-result-notes">
                    <strong>Notes:</strong> ${testResult.notes}
                </div>
            `;
        }

        if (testResult.componentTestResults && testResult.componentTestResults.length > 0) {
            html += `
                <div class="component-test-results">
                    <h4>Component Tests</h4>
                    <table class="component-tests-table">
                        <thead>
                            <tr>
                                <th>Component</th>
                                <th>Result Value</th>
                                <th>Notes</th>
                            </tr>
                        </thead>
                        <tbody>
            `;
            testResult.componentTestResults.forEach(component => {
                html += `
                    <tr>
                        <td>${component.componentTestResultName}</td>
                        <td>${component.resultValue}</td>
                        <td>${component.notes || '-'}</td>
                    </tr>
                `;
            });
            html += `
                        </tbody>
                    </table>
                </div>
            `;
        }

        html += `</div>`;
    });
    
    html += `</div>`;
    section.innerHTML = html;
}

// Render ARV regimens with embedded medications
function renderARVRegimens(regimens) {
    const section = document.getElementById('arvRegimensContent');
    
    if (!regimens || regimens.length === 0) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-pills"></i>
                <p>No ARV regimens found.</p>
                <p class="empty-state-subtitle">ARV treatment regimens will appear here when prescribed by your doctor.</p>
            </div>
        `;
        return;
    }

    let html = `
        <div class="regimens-stats">
            <div class="stat-card">
                <div class="stat-number">${regimens.length}</div>
                <div class="stat-label">Total Regimens</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${regimens.filter(r => r.regimenStatus === 2).length}</div>
                <div class="stat-label">Active</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${regimens.filter(r => r.regimenStatus === 5).length}</div>
                <div class="stat-label">Completed</div>
            </div>
        </div>
        <div class="regimens-grid">
    `;
    
    regimens.forEach(regimen => {
        let statusClass = '';
        switch (regimen.regimenStatus) {
            case 1: statusClass = 'regimen-planned'; break;
            case 2: statusClass = 'regimen-active'; break;
            case 3: statusClass = 'regimen-paused'; break;
            case 4: statusClass = 'regimen-failed'; break;
            case 5: statusClass = 'regimen-completed'; break;
            default: statusClass = 'regimen-unknown'; break;
        }
        
        const statusText = regimenStatusMap[regimen.regimenStatus] || regimen.regimenStatus;
        const levelText = regimenLevelMap[regimen.regimenLevel] || regimen.regimenLevel;
        
        // Use embedded medications from the regimen
        const regimenMeds = regimen.arvMedications || [];
        
        html += `
            <div class="regimen-card">
                <div class="regimen-header">
                    <span class="regimen-id">Regimen ID: ${regimen.patientArvRegiId}</span>
                    <span class="regimen-status ${statusClass}">${statusText}</span>
                </div>
                <div class="regimen-details">
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Start Date</div>
                        <div class="regimen-detail-value">${regimen.startDate}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">End Date</div>
                        <div class="regimen-detail-value">${regimen.endDate || 'Ongoing'}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Regimen Level</div>
                        <div class="regimen-detail-value">${levelText}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Total Cost</div>
                        <div class="regimen-detail-value">${regimen.totalCost ? regimen.totalCost.toLocaleString() + ' VND' : 'Not specified'}</div>
                    </div>
                    <div class="regimen-detail">
                        <div class="regimen-detail-label">Created At</div>
                        <div class="regimen-detail-value">${new Date(regimen.createdAt).toLocaleDateString()}</div>
                    </div>
                </div>
                ${regimen.notes ? `
                    <div class="regimen-notes">
                        <strong>Notes:</strong> ${regimen.notes}
                    </div>
                ` : ''}
                
                ${regimen.totalCost && regimen.totalCost > 0 ? `
                    <div class="regimen-payment-section">
                        <div class="payment-info">
                            <span class="payment-label">Payment Required:</span>
                            <span class="payment-amount">${regimen.totalCost.toLocaleString()} VND</span>
                        </div>
                        <button class="btn-pay-regimen" onclick="initiatePayment(${regimen.patientArvRegiId}, ${regimen.totalCost}, 'ARV Regimen Payment - ID: ${regimen.patientArvRegiId}')">
                            <i class="fas fa-credit-card"></i> Pay Now
                        </button>
                    </div>
                ` : ''}
                
                <div class="regimen-medications">
                    <h4>Medications (${regimenMeds.length})</h4>
                    ${regimenMeds.length > 0 ? `
                        <table class="medications-table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Dosage</th>
                                    <th>Quantity</th>
                                    <th>Unit Price</th>
                                    <th>Subtotal</th>
                                    <th>Manufacturer</th>
                                    <th>Description</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${regimenMeds.map(med => `
                                    <tr>
                                        <td data-label="Name"><strong>${med.medicationDetail.arvMedicationName}</strong></td>
                                        <td data-label="Dosage">${med.medicationDetail.arvMedicationDosage}</td>
                                        <td data-label="Quantity">${med.quantity}</td>
                                        <td data-label="Unit Price">${med.medicationDetail.arvMedicationPrice ? med.medicationDetail.arvMedicationPrice.toLocaleString() + ' VND' : 'N/A'}</td>
                                        <td data-label="Subtotal">${med.medicationDetail.arvMedicationPrice ? (med.medicationDetail.arvMedicationPrice * med.quantity).toLocaleString() + ' VND' : 'N/A'}</td>
                                        <td data-label="Manufacturer">${med.medicationDetail.arvMedicationManufacturer}</td>
                                        <td data-label="Description"><small>${med.medicationDetail.arvMedicationDescription}</small></td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    ` : `<div class='empty-state'><i class='fas fa-capsules'></i> No medications for this regimen.</div>`}
                </div>
            </div>
        `;
    });
    
    html += `</div>`;
    section.innerHTML = html;
}

// Main function to load all patient data
async function loadPatientData() {
    // Show loading state
    document.body.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Loading your medical records...</p>
        </div>
    `;

    try {
        // Fetch medical record data
        const medicalData = await fetchPatientMedicalData();
        
        // Fetch ARV regimens (new API)
        const arvRegimens = await fetchPatientArvRegimens();

        // Render the page structure
        document.body.innerHTML = `
            <div class="medical-record-container">
                <div class="page-header">
                    <h1>My Medical Records</h1>
                    <button class="btn-back" onclick="window.history.back()">
                        <i class="fas fa-arrow-left"></i> Back
                    </button>
                </div>
                
                <!-- Section Navigation -->
                <div class="section-navigation">
                    <button class="nav-btn active" onclick="showSection('appointments')" id="appointmentsNav">
                        <i class="fas fa-calendar-alt"></i> Appointments
                    </button>
                    <button class="nav-btn" onclick="showSection('testResults')" id="testResultsNav">
                        <i class="fas fa-flask"></i> Test Results
                    </button>
                    <button class="nav-btn" onclick="showSection('arvRegimens')" id="arvRegimensNav">
                        <i class="fas fa-pills"></i> ARV Regimens
                    </button>
                </div>
                
                <div class="medical-record-content">
                    <!-- Appointments Section -->
                    <section class="medical-section active" id="appointmentsSection">
                        <div class="section-header">
                            <h2><i class="fas fa-calendar-alt"></i> Appointments</h2>
                            <p class="section-description">View your scheduled and completed appointments</p>
                        </div>
                        <div id="appointmentsContent"></div>
                    </section>
                    
                    <!-- Test Results Section -->
                    <section class="medical-section" id="testResultsSection">
                        <div class="section-header">
                            <h2><i class="fas fa-flask"></i> Test Results</h2>
                            <p class="section-description">View your laboratory test results and reports</p>
                        </div>
                        <div id="testResultsContent"></div>
                    </section>
                    
                    <!-- ARV Regimens Section -->
                    <section class="medical-section" id="arvRegimensSection">
                        <div class="section-header">
                            <h2><i class="fas fa-pills"></i> ARV Regimens</h2>
                            <p class="section-description">View your current and past ARV treatment regimens</p>
                        </div>
                        <div id="arvRegimensContent"></div>
                    </section>
                </div>
            </div>
        `;

        // Check if we have medical data
        if (medicalData && medicalData.appointments && medicalData.appointments.length > 0) {
            // Render appointments
            renderAppointments(medicalData.appointments);
            
            // Render test results
            renderTestResults(medicalData.testResults);
        } else {
            // No appointments found
            document.getElementById('appointmentsContent').innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-calendar-times"></i>
                    <p>No appointments found.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Book an Appointment
                    </button>
                </div>
            `;
            
            // No test results
            document.getElementById('testResultsContent').innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-flask"></i>
                    <p>No test results found.</p>
                    <p class="empty-state-subtitle">Test results will appear here after your laboratory tests.</p>
                </div>
            `;
        }

        // Render ARV regimens (using new API)
        renderARVRegimens(arvRegimens);

        // Add section navigation functionality
        window.showSection = function(sectionName) {
            // Hide all sections
            document.querySelectorAll('.medical-section').forEach(section => {
                section.classList.remove('active');
            });
            
            // Remove active class from all nav buttons
            document.querySelectorAll('.nav-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            
            // Show selected section
            document.getElementById(sectionName + 'Section').classList.add('active');
            
            // Activate corresponding nav button
            document.getElementById(sectionName + 'Nav').classList.add('active');
        };

        // If no data at all, show a comprehensive message in appointments section
        if ((!medicalData || !medicalData.appointments || medicalData.appointments.length === 0) && 
            (!arvRegimens || arvRegimens.length === 0)) {
            
            document.getElementById('appointmentsContent').innerHTML = `
                <div class="no-records-container">
                    <div class="no-records-icon">
                        <i class="fas fa-clipboard-list"></i>
                    </div>
                    <h3>No Medical Records Found</h3>
                    <p>You don't have any medical records yet. Medical records will appear here after your appointments with doctors.</p>
                    <div class="suggestions">
                        <p><strong>What you can do:</strong></p>
                        <ul>
                            <li>Book an appointment with a doctor</li>
                            <li>Complete your scheduled appointments</li>
                            <li>Your medical records will be created after consultations</li>
                        </ul>
                    </div>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Book an Appointment
                    </button>
                </div>
            `;
        }

    } catch (error) {
        console.error('Error loading patient data:', error);
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Error loading medical records. Please try again.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Try Again
                </button>
            </div>
        `;
    }
}

// Initialize page when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Load payment scripts
    loadPaymentScripts();
    
    // Load patient data
    loadPatientData();
});

// Load payment scripts dynamically
function loadPaymentScripts() {
    // Load payment CSS
    const paymentCSS = document.createElement('link');
    paymentCSS.rel = 'stylesheet';
    paymentCSS.href = '../payment/payment.css';
    document.head.appendChild(paymentCSS);
    
    // Load payment JS
    const paymentJS = document.createElement('script');
    paymentJS.src = '../payment/payment.js';
    paymentJS.onload = function() {
        console.log('Payment system loaded successfully');
    };
    document.head.appendChild(paymentJS);
}

// Initiate payment for ARV regimen
function initiatePayment(regimenId, amount, description) {
    // Ensure payment system is loaded
    if (!window.PaymentModal || !window.paymentService) {
        alert('Payment system is loading. Please try again in a moment.');
        return;
    }
    
    const paymentData = {
        pmrId: regimenId, // Using regimen ID as PMR ID
        srvId: 1, // Service ID for ARV regimen (you may need to adjust this)
        amount: amount,
        currency: 'VND',
        paymentMethod: 'stripe',
        description: description
    };
    
    // Create and show payment modal
    const paymentModal = new window.PaymentModal(window.paymentService);
    paymentModal.show(paymentData);
}

// Make function global for onclick access
window.initiatePayment = initiatePayment;