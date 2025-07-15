document.addEventListener('DOMContentLoaded', function () {
    const debugDiv = document.getElementById('dashboardDebug');
    function debug(msg) {
        if (debugDiv) debugDiv.innerText = msg;
        console.log('[Dashboard Debug]', msg);
    }

    debug('Script loaded.');
    // Get doctorId and token from localStorage
    const doctorId = localStorage.getItem('accId');
    const token = localStorage.getItem('token');
    debug('accId: ' + doctorId + ', token: ' + (token ? '[present]' : '[missing]'));

    if (!doctorId || !token) {
        debug('Doctor ID or token not found. Please log in again.');
        return;
    }

    debug('Fetching: https://localhost:7009/api/Dashboard/doctor/' + doctorId);
    fetch(`https://localhost:7009/api/Dashboard/doctor/${doctorId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            debug('Fetch response status: ' + response.status);
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            debug('Data loaded: ' + JSON.stringify(data));
            // Metrics
            document.getElementById('todayAppointments').textContent = data.todayAppointments || 0;
            document.getElementById('weeklyAppointments').textContent = data.weeklyAppointments || 0;
            document.getElementById('monthlyAppointments').textContent = data.monthlyAppointments || 0;
            document.getElementById('totalPatients').textContent = data.totalPatients || 0;
            document.getElementById('upcomingAppointments').textContent = data.upcomingAppointments || 0;
            document.getElementById('completedAppointments').textContent = data.completedAppointments || 0;

            // Chart.js: Appointments Overview
            if (window.appointmentsChart && typeof window.appointmentsChart.destroy === 'function') {
                window.appointmentsChart.destroy();
            }
            const ctx = document.getElementById('appointmentsChart').getContext('2d');
            window.appointmentsChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Today', 'This Week', 'This Month', 'Completed', 'Upcoming'],
                    datasets: [{
                        label: 'Appointments',
                        data: [
                            data.todayAppointments || 0,
                            data.weeklyAppointments || 0,
                            data.monthlyAppointments || 0,
                            data.completedAppointments || 0,
                            data.upcomingAppointments || 0
                        ],
                        backgroundColor: [
                            '#e74c3c',
                            '#3498db',
                            '#764ba2',
                            '#27ae60',
                            '#f1c40f'
                        ],
                        borderRadius: 8,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: false },
                        title: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
            debug('Chart rendered.');

            // Today's Schedule
            const scheduleDiv = document.getElementById('todaySchedule');
            scheduleDiv.innerHTML = '';
            if (Array.isArray(data.todaySchedule) && data.todaySchedule.length > 0) {
                const ul = document.createElement('ul');
                data.todaySchedule.forEach(item => {
                    // Fallback for missing fields
                    const patientName = item.patientName || 'Unknown';
                    const time = item.time || item.appointmentTime || '';
                    const reason = item.reason || '';
                    let text = time ? `${time} - ${patientName}` : patientName;
                    if (reason) text += ` (${reason})`;
                    const li = document.createElement('li');
                    li.textContent = text;
                    ul.appendChild(li);
                });
                scheduleDiv.appendChild(ul);
            } else {
                scheduleDiv.innerHTML = '<p>No schedule for today.</p>';
            }
            debug('Schedule rendered.');

            // Recent Patients
            const recentPatientsBody = document.getElementById('recentPatientsBody');
            recentPatientsBody.innerHTML = '';
            if (Array.isArray(data.recentPatients) && data.recentPatients.length > 0) {
                data.recentPatients.forEach(patient => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${patient.patientName || 'Unknown'}</td>
                        <td>${patient.lastVisit || ''}</td>
                        <td>${patient.lastVisitTime || ''}</td>
                    `;
                    recentPatientsBody.appendChild(tr);
                });
            } else {
                const tr = document.createElement('tr');
                tr.innerHTML = '<td colspan="3">No recent patients.</td>';
                recentPatientsBody.appendChild(tr);
            }
            debug('Recent patients rendered.');
            // Remove the visible debug message on success, but keep it in the console
            if (debugDiv) debugDiv.innerText = '';
            console.log('[Dashboard Debug]', 'Dashboard loaded successfully.');
        })
        .catch(error => {
            debug('Error: ' + error.message);
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
