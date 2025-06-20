document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem("token");
    const appointmentList = document.getElementById('appointment-list');
    appointmentList.innerHTML = "<p>Loading...</p>";

    try {
        const res = await fetch("https://localhost:7009/api/Appointment/my-appointments", {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Failed to fetch appointments");
        const data = await res.json();

        if (data.length === 0) {
            appointmentList.innerHTML = "<p>No appointments found.</p>";
            return;
        }

        appointmentList.innerHTML = data.map(appt => `
            <div class="appointment-card">
                <h3>Dr. ${appt.doctorName}</h3>
                <p><strong>Date:</strong> ${appt.apmtDate}</p>
                <p><strong>Time:</strong> ${appt.apmTime}</p>
                <p><strong>Status:</strong> ${renderStatus(appt.apmStatus)}</p>
                <p><strong>Notes:</strong> ${appt.notes || "None"}</p>
            </div>
        `).join('');
    } catch (err) {
        appointmentList.innerHTML = `<p style="color:red;">${err.message}</p>`;
    }
});

function renderStatus(status) {
    switch (status) {
        case 1: return '<span style="color:orange;">Pending</span>';
        case 2: return '<span style="color:green;">Confirmed</span>';
        case 3: return '<span style="color:red;">Cancelled</span>';
        default: return 'Unknown';
    }
}