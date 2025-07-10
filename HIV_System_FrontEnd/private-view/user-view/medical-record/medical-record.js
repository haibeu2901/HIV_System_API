document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");

    // --- Appointments ---
    fetch("https://localhost:7009/api/PatientMedicalRecord/GetPersonalMedicalRecord", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        const list = document.getElementById('appointments-list');
        if (!data || !data.appointments || !data.appointments.length) {
            list.innerHTML = "<p>No appointments found.</p>";
            return;
        }
        list.innerHTML = data.appointments.map(apm => `
            <div class="record-card">
                <div>
                    <b>Date:</b> ${apm.apmtDate || apm.apmtDate || apm.apmDate || apm.apmtDate || apm.apmtDate}
                    <br><b>Time:</b> ${apm.apmTime || apm.apmtTime || apm.apmtTime || apm.apmtTime || apm.apmtTime}
                    <br><b>Doctor:</b> ${apm.doctorName}
                    <br><b>Status:</b> ${renderApmStatus(apm.apmStatus)}
                </div>
                <div class="record-notes">${apm.notes || ""}</div>
            </div>
        `).join('');
    });

    // --- Test Results ---
    fetch("https://localhost:7009/api/TestResult/personal", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        const list = document.getElementById('test-results-list');
        if (!data || !data.length) {
            // If API returns a single object, wrap in array
            if (data && data.result !== undefined) data = [data];
            else {
                list.innerHTML = "<p>No test results found.</p>";
                return;
            }
        }
        list.innerHTML = data.map(tr => `
            <div class="record-card">
                <div>
                    <b>Date:</b> ${tr.testDate}
                    <br><b>Result:</b> ${tr.result ? "Positive" : "Negative"}
                </div>
                <div class="record-notes">${tr.notes || ""}</div>
            </div>
        `).join('');
    });

    // --- ARV Regimens ---
    fetch("https://localhost:7009/api/PatientArvRegimen/GetPersonalArvRegimens", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        const list = document.getElementById('arv-regimens-list');
        if (!data || !data.length) {
            list.innerHTML = "<p>No ARV regimens found.</p>";
            return;
        }
        list.innerHTML = data.map(reg => `
            <div class="record-card">
                <div>
                    <b>Notes:</b> ${reg.notes}
                    <br><b>Start:</b> ${reg.startDate}
                    <br><b>End:</b> ${reg.endDate ? reg.endDate : "Ongoing"}
                    <br><b>Status:</b> ${renderRegimenStatus(reg.regimenStatus)}
                    <br><b>Total Cost:</b> ${reg.totalCost ? reg.totalCost.toLocaleString() + " VND" : ""}
                </div>
            </div>
        `).join('');
    });

    // --- Helpers ---
    function renderApmStatus(status) {
        switch(status) {
            case 1: return "Scheduled";
            case 2: return "Completed";
            case 3: return "Cancelled";
            default: return status;
        }
    }
    function renderRegimenStatus(status) {
        switch(status) {
            case 1: return "Ongoing";
            case 2: return "Completed";
            case 3: return "Stopped";
            default: return status;
        }
    }
});