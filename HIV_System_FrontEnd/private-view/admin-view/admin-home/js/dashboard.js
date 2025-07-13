// Dashboard Module
class DashboardManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load dashboard data
    async loadDashboardData() {
        try {
            // Load statistics
            await Promise.all([
                this.loadPatientCount(),
                this.loadDoctorCount(),
                this.loadAppointmentCount(),
                this.loadRecentActivity()
            ]);
        } catch (error) {
            console.error('Error loading dashboard data:', error);
        }
    }

    async loadPatientCount() {
        const token = this.authManager.getToken();
        try {
            const response = await fetch('https://localhost:7009/api/Patient/GetAllPatients', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const patients = await response.json();
                document.getElementById('total-patients').textContent = patients.length;
            }
        } catch (error) {
            document.getElementById('total-patients').textContent = 'N/A';
        }
    }

    async loadDoctorCount() {
        const token = this.authManager.getToken();
        try {
            const response = await fetch('https://localhost:7009/api/Doctor/GetAllDoctors', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const doctors = await response.json();
                document.getElementById('total-doctors').textContent = doctors.length;
            }
        } catch (error) {
            document.getElementById('total-doctors').textContent = 'N/A';
        }
    }

    async loadAppointmentCount() {
        const token = this.authManager.getToken();
        try {
            const response = await fetch('https://localhost:7009/api/Appointment/GetAllAppointments', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const appointments = await response.json();
                document.getElementById('total-appointments').textContent = appointments.length;
                
                // Count pending appointments
                const pending = appointments.filter(apt => apt.status === 'Pending').length;
                document.getElementById('pending-appointments').textContent = pending;
            }
        } catch (error) {
            document.getElementById('total-appointments').textContent = 'N/A';
            document.getElementById('pending-appointments').textContent = 'N/A';
        }
    }

    async loadRecentActivity() {
        const activityList = document.getElementById('recent-activity-list');
        
        // Mock recent activity data
        const activities = [
            { type: 'appointment', title: 'New appointment booked', time: '2 minutes ago' },
            { type: 'patient', title: 'New patient registered', time: '15 minutes ago' },
            { type: 'doctor', title: 'Doctor schedule updated', time: '1 hour ago' },
            { type: 'system', title: 'System maintenance completed', time: '3 hours ago' }
        ];
        
        activityList.innerHTML = activities.map(activity => `
            <div class="activity-item">
                <div class="activity-icon">
                    <i class="fas fa-${this.getActivityIcon(activity.type)}"></i>
                </div>
                <div class="activity-content">
                    <div class="activity-title">${activity.title}</div>
                    <div class="activity-time">${activity.time}</div>
                </div>
            </div>
        `).join('');
    }

    getActivityIcon(type) {
        const icons = {
            appointment: 'calendar-plus',
            patient: 'user-plus',
            doctor: 'user-md',
            system: 'cog'
        };
        return icons[type] || 'info-circle';
    }
}

// Export for use in other modules
window.DashboardManager = DashboardManager;
