document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/doctor/dashboard')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            // Metrics
            document.getElementById('todayAppointments').textContent = data.todayAppointments || 0;
            document.getElementById('weeklyAppointments').textContent = data.weeklyAppointments || 0;
            document.getElementById('monthlyAppointments').textContent = data.monthlyAppointments || 0;
            document.getElementById('totalPatients').textContent = data.totalPatients || 0;
            document.getElementById('upcomingAppointments').textContent = data.upcomingAppointments || 0;
            document.getElementById('completedAppointments').textContent = data.completedAppointments || 0;

            // Today's Schedule
            const scheduleDiv = document.getElementById('todaySchedule');
            scheduleDiv.innerHTML = '';
            if (data.todaySchedule && data.todaySchedule.length > 0) {
                const ul = document.createElement('ul');
                data.todaySchedule.forEach(item => {
                    const li = document.createElement('li');
                    li.textContent = `${item.time} - ${item.patientName} (${item.reason || 'General'})`;
                    ul.appendChild(li);
                });
                scheduleDiv.appendChild(ul);
            } else {
                scheduleDiv.innerHTML = '<p>No schedule for today.</p>';
            }

            // Recent Patients
            const recentPatientsBody = document.getElementById('recentPatientsBody');
            recentPatientsBody.innerHTML = '';
            if (data.recentPatients && data.recentPatients.length > 0) {
                data.recentPatients.forEach(patient => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${patient.name}</td>
                        <td>${patient.lastVisitDate}</td>
                        <td>${patient.lastVisitTime}</td>
                    `;
                    recentPatientsBody.appendChild(tr);
                });
            } else {
                const tr = document.createElement('tr');
                tr.innerHTML = '<td colspan="3">No recent patients.</td>';
                recentPatientsBody.appendChild(tr);
            }
        })
        .catch(error => {
            // Show error state for all sections
            document.getElementById('todayAppointments').textContent = '-';
            document.getElementById('weeklyAppointments').textContent = '-';
            document.getElementById('monthlyAppointments').textContent = '-';
            document.getElementById('totalPatients').textContent = '-';
            document.getElementById('upcomingAppointments').textContent = '-';
            document.getElementById('completedAppointments').textContent = '-';
            document.getElementById('todaySchedule').innerHTML = '<p style="color:red;">Failed to load schedule.</p>';
            document.getElementById('recentPatientsBody').innerHTML = '<tr><td colspan="3" style="color:red;">Failed to load recent patients.</td></tr>';
        });
});
