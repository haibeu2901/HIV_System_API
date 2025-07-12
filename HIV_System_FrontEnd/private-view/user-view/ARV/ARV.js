document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const arvList = document.getElementById('arv-list');

    fetch("https://localhost:7009/api/PatientArvRegimen/GetPersonalArvRegimens", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        if (!data || !data.length) {
            arvList.innerHTML = "<p>No ARV regimens found.</p>";
            return;
        }
        arvList.innerHTML = data.map(reg => `
            <div class="arv-card">
                <div class="arv-notes"><b>Notes:</b> ${reg.notes}</div>
                <div class="arv-meta">
                    <b>Start:</b> ${reg.startDate} &nbsp; 
                    <b>End:</b> ${reg.endDate ? reg.endDate : "Ongoing"}
                </div>
                <div class="arv-meta">
                    <b>Level:</b> ${reg.regimenLevel} &nbsp; 
                    <span class="arv-status ${renderStatusClass(reg.regimenStatus)}">${renderStatusText(reg.regimenStatus)}</span>
                </div>
                <div class="arv-cost">
                    <i class="fa-solid fa-coins"></i> ${reg.totalCost ? reg.totalCost.toLocaleString() + " VND" : ""}
                </div>
            </div>
        `).join('');
    })
    .catch(() => {
        arvList.innerHTML = "<p style='color:red;'>Error loading ARV regimens.</p>";
    });

    function renderStatusText(status) {
        switch(status) {
            case 1: return "Ongoing";
            case 2: return "Completed";
            case 3: return "Stopped";
            default: return status;
        }
    }
  function renderStatusClass(status) {
        switch(status) {
            case 1: return "";
            case 2: return "completed";
            case 3: return "stopped";
            default: return "";
        }
    }
});