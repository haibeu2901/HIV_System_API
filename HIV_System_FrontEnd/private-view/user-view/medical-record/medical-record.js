// Render medical records with labeled options
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const recordList = document.getElementById('record-list');

    // Show loading state
    recordList.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Loading your medical records...</p>
        </div>
    `;

    fetch("https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => {
        if (res.status === 404) {
            // Handle case where user has no medical records yet
            recordList.innerHTML = `
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
            return null;
        }
        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }
        return res.json();
    })
    .then(data => {
        if (!data) return; // Handle 404 case
        
        if (!data.appointments || !data.appointments.length) {
            recordList.innerHTML = `
                <div class="no-records-container">
                    <div class="no-records-icon">
                        <i class="fas fa-clipboard-list"></i>
                    </div>
                    <h3>No Medical Records Found</h3>
                    <p>You don't have any medical records yet. Medical records will appear here after your appointments with doctors.</p>
                    <button class="btn-book-appointment" onclick="window.location.href='../booking/appointment-booking.html'">
                        <i class="fas fa-calendar-plus"></i> Book an Appointment
                    </button>
                </div>
            `;
            return;
        }
        
        recordList.innerHTML = data.appointments.map(apm => `
            <div class="record-card">
                <div class="record-info">
                    <div><b>Doctor:</b> ${apm.doctorName}</div>
                    <div><b>Date:</b> ${apm.apmtDate}</div>
                    <div><b>Time:</b> ${apm.apmTime}</div>
                    <div><b>Notes:</b> ${apm.notes || "No notes available"}</div>
                </div>
                <div class="record-options">
                    <button class="record-option-btn" onclick="window.location.href='../appointment-view/view-appointment.html?id=${apm.appointmentId}'">View appointment</button>
                    <button class="record-option-btn" onclick="window.location.href='../test-result/test-result.html'">View test result</button>
                    <button class="record-option-btn" onclick="window.location.href='../ARV/arv-medications.html'">View ARV regimen</button>
                </div>
            </div>
        `).join("");
    })
    .catch(err => {
        console.error('Error loading medical records:', err);
        recordList.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Error Loading Medical Records</h3>
                <p>We encountered an issue while loading your medical records. Please try again later.</p>
                <button class="btn-retry" onclick="window.location.reload()">
                    <i class="fas fa-refresh"></i> Try Again
                </button>
            </div>
        `;
    });
});