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
                    <h3>No ARV Regimens Found</h3>
                    <p>You don't have any ARV regimens yet. ARV regimens will be prescribed by your doctor based on your medical condition and test results.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Book an Appointment
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
                        <span>Regimen ID: ${reg.patientArvRegiId}</span>
                    </div>
                    <div class="arv-status-badge">
                        <span class="arv-status ${renderStatusClass(reg.regimenStatus)}">${renderStatusText(reg.regimenStatus)}</span>
                    </div>
                </div>
                
                <div class="arv-notes">
                    <i class="fas fa-notes-medical"></i>
                    <strong>Notes:</strong> ${reg.notes || 'No notes available'}
                </div>
                
                <div class="arv-meta">
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-start"></i>
                        <strong>Start Date:</strong> ${formatDate(reg.startDate)}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-end"></i>
                        <strong>End Date:</strong> ${reg.endDate ? formatDate(reg.endDate) : '<span class="ongoing">Ongoing</span>'}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-layer-group"></i>
                        <strong>Level:</strong> ${reg.regimenLevel}
                    </div>
                    <div class="arv-meta-item">
                        <i class="fas fa-calendar-plus"></i>
                        <strong>Created:</strong> ${formatDate(reg.createdAt)}
                    </div>
                </div>
                
                <div class="arv-cost">
                    <i class="fas fa-coins"></i>
                    <strong>Total Cost:</strong> ${reg.totalCost ? reg.totalCost.toLocaleString() + " VND" : "Not specified"}
                </div>
                
                <div class="arv-medications">
                    <h4><i class="fas fa-pills"></i> Medications (${reg.arvMedications?.length || 0})</h4>
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
                                            <span><strong>Manufacturer:</strong> ${med.medicationDetail.arvMedicationManufacturer}</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-hashtag"></i>
                                            <span><strong>Quantity:</strong> ${med.quantity}</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-money-bill"></i>
                                            <span><strong>Price:</strong> ${med.medicationDetail.arvMedicationPrice?.toLocaleString()} VND</span>
                                        </div>
                                        <div class="detail-item">
                                            <i class="fas fa-calculator"></i>
                                            <span><strong>Subtotal:</strong> ${(med.medicationDetail.arvMedicationPrice * med.quantity).toLocaleString()} VND</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        `).join('') || '<p class="no-medications">No medications listed</p>'}
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
                <h3>Error Loading ARV Regimens</h3>
                <p>We encountered an issue while loading your ARV regimens. Please try again later.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Try Again
                </button>
            </div>
        `;
    });

    function formatDate(dateString) {
        if (!dateString) return 'Not specified';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-GB', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    function renderStatusText(status) {
        switch(status) {
            case 1: return "Active";
            case 2: return "Completed";
            case 3: return "Stopped";
            default: return `Status ${status}`;
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