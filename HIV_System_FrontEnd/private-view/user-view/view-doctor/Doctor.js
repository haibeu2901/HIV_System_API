document.addEventListener("DOMContentLoaded", async () => {
    const token = localStorage.getItem("token");
    const doctorListDiv = document.getElementById("doctor-list");

    try {
        const response = await fetch("https://localhost:7009/api/Doctor/GetAllDoctors", {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error("Failed to fetch doctors");
        const doctors = await response.json();

        if (doctors.length === 0) {
            doctorListDiv.innerHTML = "<p>No doctors found.</p>";
            return;
        }

        doctorListDiv.innerHTML = doctors.map(doc => `
            <div class="doctor-card">
                <div class="doctor-header">
                    <div class="doctor-avatar">
                        <i class="fas fa-user-md"></i>
                    </div>
                    <div>
                        <h3>${doc.account.fullname}</h3>
                        <p class="degree">${doc.degree}</p>
                        <p class="email">${doc.account.email}</p>
                    </div>
                </div>
                <p class="bio">${doc.bio}</p>
                <div class="schedule">
                    <strong>Work Schedule:</strong>
                    <ul>
                        ${doc.workSchedule.map(ws => `
                            <li>
                                ${["Sun","Mon","Tue","Wed","Thu","Fri","Sat"][ws.dayOfWeek]}: 
                                ${ws.startTime.substring(0,5)} - ${ws.endTime.substring(0,5)}
                            </li>
                        `).join("")}
                    </ul>
                </div>
                <div class="doctor-actions">
                    <a class="btn-chat" href="mailto:${doc.account.email}?subject=Contact%20from%20CareFirst%20HIV%20Clinic" target="_blank">
                        <i class="fas fa-envelope"></i> Contact the Doctor
                    </a>
=======
=======
                    <button class="btn-chat" onclick="chatToDoctor('${doc.account.accId}', '${doc.account.fullname}')">
                        <i class="fas fa-comments"></i> Chat to Doctor
                    </button>
                    <button class="btn-book" onclick="bookDoctor('${doc.doctorId}')">
                        <i class="fas fa-calendar-plus"></i> Book Appointment
                    </button>
                </div>
            </div>
        `).join("");
    } catch (err) {
        doctorListDiv.innerHTML = `<p style="color:red;">${err.message}</p>`;
    }
});

// Add these functions globally
function chatToDoctor(doctorId, doctorName) {
    // Redirect to chat page (implement your chat page as needed)
    window.location.href = `/private-view/user-view/chat/chat.html?doctorId=${doctorId}&doctorName=${encodeURIComponent(doctorName)}`;
}

function bookDoctor(doctorId) {
    // Redirect to booking page with doctorId as query param
    window.location.href = `/private-view/user-view/booking/appointment-booking.html?doctorId=${doctorId}`;
}
function chatToDoctor(doctorId, doctorName) {
    // Show modal
    document.getElementById('chat-modal').style.display = 'flex';
    document.getElementById('chat-doctor-name').textContent = `Chat with Dr. ${doctorName}`;
    document.getElementById('chat-messages').innerHTML = '<div style="color:#888;">Start your conversation...</div>';
    document.getElementById('chat-input').value = '';
    // Optionally, store doctorId for sending messages
    document.getElementById('chat-send-btn').onclick = function() {
        const msg = document.getElementById('chat-input').value.trim();
        if (msg) {
            const chatBox = document.getElementById('chat-messages');
            chatBox.innerHTML += `<div><b>You:</b> ${msg}</div>`;
            document.getElementById('chat-input').value = '';
            chatBox.scrollTop = chatBox.scrollHeight;
            // Here you can implement sending the message to backend if needed
        }
    };
}

// Close modal logic
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('chat-close').onclick = function() {
        document.getElementById('chat-modal').style.display = 'none';
    };
    // Optional: close on outside click
    document.getElementById('chat-modal').onclick = function(e) {
        if (e.target === this) this.style.display = 'none';
    };
});