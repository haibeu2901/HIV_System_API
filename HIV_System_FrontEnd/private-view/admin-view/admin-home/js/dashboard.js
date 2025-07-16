// Dashboard Module
class DashboardManager {
    constructor(authManager) {
        this.authManager = authManager;
        this.charts = {};
    }

    // Load dashboard data
    async loadDashboardData() {
        console.log('🔄 Loading dashboard data...');
        
        try {
            const token = this.authManager.getToken();
            const userRole = parseInt(localStorage.getItem('userRole'));
            const userId = localStorage.getItem('accId') || '1';
            
            // Determine API endpoint based on user role
            let apiEndpoint;
            if (userRole === 5) {
                // Manager role - use manager API
                apiEndpoint = `https://localhost:7009/api/Dashboard/manager?userId=${userId}`;
                console.log('📊 Loading manager dashboard data for userId:', userId);
            } else {
                // Admin role - use admin API
                apiEndpoint = `https://localhost:7009/api/Dashboard/admin?userId=${userId}`;
                console.log('📊 Loading admin dashboard data for userId:', userId);
            }
            
            console.log('🌐 Fetching data from:', apiEndpoint);
            
            const response = await fetch(apiEndpoint, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });

            if (response.ok) {
                const data = await response.json();
                console.log('✅ Dashboard data loaded successfully:', data);
                
                // Update statistics cards
                this.updateStatisticsCards(data);
                
                // Update charts
                this.updateUserDistributionChart(data.userDistribution);
                this.updateRevenueChart(data);
                this.updateAppointmentChart(data);
                
                // Update recent activities
                this.updateRecentActivities(data.recentActivities);
                
                console.log('✅ Dashboard updated successfully');
                
            } else {
                console.error('❌ HTTP error:', response.status);
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('❌ Error loading dashboard data:', error);
            this.showErrorState();
        }
    }

    // Update statistics cards
    updateStatisticsCards(data) {
        console.log('📈 Updating statistics cards with data:', data);
        
        const stats = [
            { id: 'total-users', value: data.totalUsers, label: 'Total Users' },
            { id: 'total-patients', value: data.totalPatients, label: 'Total Patients' },
            { id: 'total-doctors', value: data.totalDoctors, label: 'Total Doctors' },
            { id: 'total-staff', value: data.totalStaff, label: 'Total Staff' },
            { id: 'total-appointments', value: data.totalAppointments, label: 'Total Appointments' },
            { id: 'pending-appointments', value: data.pendingAppointments, label: 'Pending Appointments' },
            { id: 'total-services', value: data.totalServices, label: 'Total Services' },
            { id: 'total-revenue', value: this.formatCurrency(data.totalRevenue), label: 'Total Revenue' },
            { id: 'monthly-revenue', value: this.formatCurrency(data.monthlyRevenue), label: 'Monthly Revenue' }
        ];

        stats.forEach(stat => {
            const element = document.getElementById(stat.id);
            if (element) {
                element.textContent = stat.value;
                element.classList.add('stat-updated');
                console.log(`✅ Updated ${stat.label}: ${stat.value}`);
                
                // Add animation
                setTimeout(() => {
                    element.classList.remove('stat-updated');
                }, 500);
            } else {
                console.warn(`⚠️ Element not found: ${stat.id}`);
            }
        });
    }

    // Update user distribution chart
    updateUserDistributionChart(userDistribution) {
        console.log('📊 Updating user distribution chart with:', userDistribution);
        
        const ctx = document.getElementById('userDistributionChart');
        if (!ctx) {
            console.warn('⚠️ User distribution chart canvas not found');
            return;
        }

        // Destroy existing chart if it exists
        if (this.charts.userDistribution) {
            this.charts.userDistribution.destroy();
        }

        if (!userDistribution || Object.keys(userDistribution).length === 0) {
            console.warn('⚠️ No user distribution data available');
            return;
        }

        const labels = Object.keys(userDistribution);
        const data = Object.values(userDistribution);
        const colors = [
            '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF'
        ];

        console.log('📊 Creating user distribution chart with labels:', labels, 'data:', data);

        this.charts.userDistribution = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors.slice(0, labels.length),
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
        
        console.log('✅ User distribution chart created successfully');
    }

    // Update revenue chart
    updateRevenueChart(data) {
        const ctx = document.getElementById('revenueChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (this.charts.revenue) {
            this.charts.revenue.destroy();
        }

        // Generate mock monthly data for the chart
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const currentMonth = new Date().getMonth();
        const monthlyData = months.map((month, index) => {
            if (index <= currentMonth) {
                return index === currentMonth ? data.monthlyRevenue : Math.floor(Math.random() * 1000000) + 500000;
            }
            return 0;
        });

        this.charts.revenue = new Chart(ctx, {
            type: 'line',
            data: {
                labels: months,
                datasets: [{
                    label: 'Monthly Revenue',
                    data: monthlyData,
                    borderColor: '#36A2EB',
                    backgroundColor: 'rgba(54, 162, 235, 0.1)',
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#36A2EB',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('vi-VN', {
                                    style: 'currency',
                                    currency: 'VND'
                                }).format(value);
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `Revenue: ${new Intl.NumberFormat('vi-VN', {
                                    style: 'currency',
                                    currency: 'VND'
                                }).format(context.raw)}`;
                            }
                        }
                    }
                }
            }
        });
    }

    // Update appointment chart
    updateAppointmentChart(data) {
        const ctx = document.getElementById('appointmentChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (this.charts.appointment) {
            this.charts.appointment.destroy();
        }

        const completedAppointments = data.totalAppointments - data.pendingAppointments;
        
        this.charts.appointment = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Completed', 'Pending'],
                datasets: [{
                    label: 'Appointments',
                    data: [completedAppointments, data.pendingAppointments],
                    backgroundColor: ['#4BC0C0', '#FF6384'],
                    borderColor: ['#4BC0C0', '#FF6384'],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `${context.label}: ${context.raw}`;
                            }
                        }
                    }
                }
            }
        });
    }

    // Update recent activities
    updateRecentActivities(activities) {
        const activityList = document.getElementById('recent-activity-list');
        if (!activityList) return;

        if (!activities || activities.length === 0) {
            activityList.innerHTML = '<div class="no-activity">No recent activities found</div>';
            return;
        }

        activityList.innerHTML = activities.map(activity => `
            <div class="activity-item">
                <div class="activity-icon">
                    <i class="fas fa-${this.getActivityIcon(activity.activityType)}"></i>
                </div>
                <div class="activity-content">
                    <div class="activity-title">${activity.description}</div>
                    <div class="activity-time">${this.formatTime(activity.createdAt)}</div>
                    <div class="activity-type">${activity.activityType}</div>
                </div>
            </div>
        `).join('');
    }

    // Get activity icon based on type
    getActivityIcon(type) {
        const icons = {
            'Appt Confirm': 'calendar-check',
            'Appointment Update': 'calendar-edit',
            'Appointment Request': 'calendar-plus',
            'User Registration': 'user-plus',
            'System Update': 'cog',
            'Payment': 'money-bill-wave'
        };
        return icons[type] || 'info-circle';
    }

    // Format time
    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInMs = now - date;
        const diffInMins = Math.floor(diffInMs / (1000 * 60));
        const diffInHours = Math.floor(diffInMins / 60);
        const diffInDays = Math.floor(diffInHours / 24);

        if (diffInMins < 1) return 'Just now';
        if (diffInMins < 60) return `${diffInMins} minute${diffInMins > 1 ? 's' : ''} ago`;
        if (diffInHours < 24) return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
        if (diffInDays < 7) return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
        
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    // Show error state
    showErrorState() {
        // Update statistics cards with error state
        const errorStats = [
            'total-users', 'total-patients', 'total-doctors', 'total-staff',
            'total-appointments', 'pending-appointments', 'total-services',
            'total-revenue', 'monthly-revenue'
        ];

        errorStats.forEach(statId => {
            const element = document.getElementById(statId);
            if (element) {
                element.textContent = 'Error';
                element.classList.add('stat-error');
            }
        });

        // Show error message in charts
        const chartContainers = ['userDistributionChart', 'revenueChart', 'appointmentChart'];
        chartContainers.forEach(chartId => {
            const container = document.getElementById(chartId);
            if (container) {
                container.innerHTML = '<div class="chart-error">Failed to load chart data</div>';
            }
        });

        // Show error message in recent activities
        const activityList = document.getElementById('recent-activity-list');
        if (activityList) {
            activityList.innerHTML = '<div class="error-message">Failed to load recent activities</div>';
        }
    }

    // Initialize charts library
    initializeCharts() {
        // Load Chart.js if not already loaded
        if (typeof Chart === 'undefined') {
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/chart.js';
            script.onload = () => {
                console.log('Chart.js loaded successfully');
                this.loadDashboardData();
            };
            document.head.appendChild(script);
        } else {
            this.loadDashboardData();
        }
    }

    // Destroy all charts
    destroyCharts() {
        Object.values(this.charts).forEach(chart => {
            if (chart) {
                chart.destroy();
            }
        });
        this.charts = {};
    }

    // Legacy methods for backward compatibility
    async loadPatientCount() {
        // This method is now handled by loadDashboardData
        console.warn('loadPatientCount is deprecated, use loadDashboardData instead');
    }

    async loadDoctorCount() {
        // This method is now handled by loadDashboardData
        console.warn('loadDoctorCount is deprecated, use loadDashboardData instead');
    }

    async loadAppointmentCount() {
        // This method is now handled by loadDashboardData
        console.warn('loadAppointmentCount is deprecated, use loadDashboardData instead');
    }

    async loadRecentActivity() {
        // This method is now handled by loadDashboardData
        console.warn('loadRecentActivity is deprecated, use loadDashboardData instead');
    }

    // Initialize dashboard
    init() {
        console.log('Initializing DashboardManager...');
        this.initializeCharts();
        
        // Set up refresh button if it exists
        const refreshButton = document.getElementById('refresh-dashboard');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => {
                console.log('Refreshing dashboard...');
                this.loadDashboardData();
            });
        }
        
        // Auto-refresh every 5 minutes
        setInterval(() => {
            console.log('Auto-refreshing dashboard...');
            this.loadDashboardData();
        }, 300000); // 5 minutes
        
        console.log('DashboardManager initialized successfully');
    }
}


// Export for use in other modules
window.DashboardManager = DashboardManager;
