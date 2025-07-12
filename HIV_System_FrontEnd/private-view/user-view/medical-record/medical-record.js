// Render medical records with labeled options
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const recordList = document.getElementById('record-list');

    fetch("https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        if (!data || !data.appointments || !data.appointments.length) {
            recordList.innerHTML = "<p>No medical records found.</p>";
            return;
        }
        recordList.innerHTML = data.appointments.map(apm => `
            <div class="record-card">
                <div class="record-info">
                    <div><b>Doctor:</b> ${apm.doctorName}</div>
                    <div><b>Date:</b> ${apm.apmtDate}</div>
                    <div><b>Time:</b> ${apm.apmTime}</div>
                    <div><b>Notes:</b> ${apm.notes || ""}</div>
                </div>
                <div class="record-options">
                    <button class="record-option-btn" onclick="window.location.href='../appointment-view/view-appointment.html?id=${apm.appointmentId}'">View appointment</button>
                    <button class="record-option-btn" onclick="window.location.href='../test-result/test-result.html'">View test result</button>
                    <button class="record-option-btn" onclick="window.location.href='../ARV/ARV.html'">View ARV regimen</button>
                </div>
            </div>
        `).join("");
    })
    .catch(err => {
        console.error(err);
        recordList.innerHTML = "<p>Error loading medical records.</p>";
    });
});