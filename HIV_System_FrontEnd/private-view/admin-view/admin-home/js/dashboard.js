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
                
                // Load detailed payment data
                await this.loadPaymentData();
                
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

    // Load detailed payment data
    async loadPaymentData() {
        console.log('💰 Loading detailed payment data...');
        
        try {
            const token = this.authManager.getToken();
            
            const response = await fetch('https://localhost:7009/api/Payment/GetAllPayments', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });

            if (response.ok) {
                const payments = await response.json();
                console.log('✅ Payment data loaded successfully:', payments);
                
                // Store payment data for use in other methods
                this.paymentData = payments;
                
                // Process and display payment data
                this.updatePaymentAnalytics(payments);
                this.updateRecentPayments(payments);
                
                // Update detailed payments list
                this.updateDetailedPaymentsList(payments);
                
                // Update revenue chart with actual payment data
                this.updateRevenueChart({ monthlyRevenue: 0 }, payments);
                
            } else {
                console.error('❌ Payment API error:', response.status);
            }
        } catch (error) {
            console.error('❌ Error loading payment data:', error);
        }
    }

    // Update payment analytics
    updatePaymentAnalytics(payments) {
        console.log('📊 Processing payment analytics...');
        
        // Calculate total revenue from actual payments
        const totalRevenue = payments
            .filter(p => p.paymentStatus === 2) // Only successful payments
            .reduce((sum, payment) => sum + payment.amount, 0);
            
        // Calculate monthly revenue (current month)
        const currentMonth = new Date().getMonth();
        const currentYear = new Date().getFullYear();
        const monthlyRevenue = payments
            .filter(p => {
                const paymentDate = new Date(p.paymentDate);
                return p.paymentStatus === 2 && 
                       paymentDate.getMonth() === currentMonth && 
                       paymentDate.getFullYear() === currentYear;
            })
            .reduce((sum, payment) => sum + payment.amount, 0);
            
        // Update revenue statistics
        const totalRevenueElement = document.getElementById('total-revenue');
        const monthlyRevenueElement = document.getElementById('monthly-revenue');
        
        if (totalRevenueElement) {
            totalRevenueElement.textContent = this.formatCurrency(totalRevenue);
            totalRevenueElement.classList.add('stat-updated');
        }
        
        if (monthlyRevenueElement) {
            monthlyRevenueElement.textContent = this.formatCurrency(monthlyRevenue);
            monthlyRevenueElement.classList.add('stat-updated');
        }
        
        // Payment method distribution
        this.updatePaymentMethodChart(payments);
        
        // Payment status distribution
        this.updatePaymentStatusChart(payments);
        
        console.log(`✅ Revenue calculated - Total: ${this.formatCurrency(totalRevenue)}, Monthly: ${this.formatCurrency(monthlyRevenue)}`);
    }

    // Update recent payments display
    updateRecentPayments(payments) {
        console.log('💳 Updating recent payments display...');
        
        // Get recent payments (last 10)
        const recentPayments = payments
            .sort((a, b) => new Date(b.paymentDate) - new Date(a.paymentDate))
            .slice(0, 10);
            
        // Find or create recent payments container
        let paymentsContainer = document.getElementById('recent-payments-list');
        if (!paymentsContainer) {
            // Create payments section if it doesn't exist
            this.createPaymentsSection();
            paymentsContainer = document.getElementById('recent-payments-list');
        }
        
        if (paymentsContainer) {
            paymentsContainer.innerHTML = recentPayments.map(payment => `
                <div class="payment-item ${this.getPaymentStatusClass(payment.paymentStatus)}">
                    <div class="payment-header">
                        <div class="payment-info">
                            <div class="payment-service">${payment.serviceName || 'ARV Medication'}</div>
                            <div class="payment-patient">${payment.patientName}</div>
                        </div>
                        <div class="payment-amount">
                            ${this.formatCurrency(payment.amount)}
                        </div>
                    </div>
                    <div class="payment-details">
                        <div class="payment-method">
                            <i class="fas fa-${this.getPaymentMethodIcon(payment.paymentMethod)}"></i>
                            ${this.formatPaymentMethod(payment.paymentMethod)}
                        </div>
                        <div class="payment-status">
                            <span class="status-badge status-${payment.paymentStatus}">
                                ${this.getPaymentStatusText(payment.paymentStatus)}
                            </span>
                        </div>
                        <div class="payment-date">
                            ${this.formatTime(payment.paymentDate)}
                        </div>
                    </div>
                    <div class="payment-description">
                        ${payment.description}
                    </div>
                </div>
            `).join('');
        }
        
        console.log(`✅ Updated recent payments display with ${recentPayments.length} payments`);
    }

    // Create payments section in the dashboard
    createPaymentsSection() {
        const mainContent = document.querySelector('.dashboard-grid') || document.querySelector('.main-content');
        if (!mainContent) return;
        
        const paymentsSection = document.createElement('div');
        paymentsSection.className = 'dashboard-card payments-section';
        paymentsSection.innerHTML = `
            <div class="card-header">
                <h3><i class="fas fa-money-bill-wave"></i> Recent Payments</h3>
                <span class="card-subtitle">Latest payment transactions</span>
            </div>
            <div class="card-content">
                <div id="recent-payments-list" class="payments-list"></div>
            </div>
        `;
        
        mainContent.appendChild(paymentsSection);
    }

    // Update detailed payments list with full control capabilities
    updateDetailedPaymentsList(payments) {
        console.log('📋 Updating detailed payments list...');
        
        // Find or create detailed payments container
        let detailedContainer = document.getElementById('detailed-payments-container');
        if (!detailedContainer) {
            this.createDetailedPaymentsSection();
            detailedContainer = document.getElementById('detailed-payments-container');
        }
        
        if (!detailedContainer) return;
        
        // Sort payments by date (newest first)
        const sortedPayments = [...payments].sort((a, b) => new Date(b.paymentDate) - new Date(a.paymentDate));
        
        // Create detailed table
        const tableHtml = `
            <div class="payments-controls">
                <div class="controls-row">
                    <div class="search-box">
                        <i class="fas fa-search"></i>
                        <input type="text" id="payment-search" placeholder="Tìm kiếm theo tên bệnh nhân, dịch vụ, hoặc mô tả...">
                    </div>
                    <div class="filter-controls">
                        <select id="status-filter">
                            <option value="">Tất cả trạng thái</option>
                            <option value="1">Đang chờ</option>
                            <option value="2">Đã thanh toán</option>
                            <option value="3">Thất bại</option>
                        </select>
                        <select id="method-filter">
                            <option value="">Tất cả phương thức</option>
                            <option value="cash">Tiền mặt</option>
                            <option value="card">Thẻ tín dụng</option>
                            <option value="transfer">Chuyển khoản</option>
                        </select>
                        <select id="date-filter">
                            <option value="">Tất cả thời gian</option>
                            <option value="today">Hôm nay</option>
                            <option value="week">Tuần này</option>
                            <option value="month">Tháng này</option>
                            <option value="year">Năm nay</option>
                        </select>
                    </div>
                    <div class="actions-controls">
                        <button id="export-payments" class="export-btn" title="Xuất Excel">
                            <i class="fas fa-file-excel"></i> Xuất
                        </button>
                        <button id="refresh-payments" class="refresh-btn" title="Làm mới">
                            <i class="fas fa-sync-alt"></i> Làm mới
                        </button>
                    </div>
                </div>
            </div>
            
            <div class="payments-stats-summary">
                <div class="stat-item">
                    <span class="stat-label">Tổng giao dịch:</span>
                    <span class="stat-value">${payments.length}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">Thành công:</span>
                    <span class="stat-value success">${payments.filter(p => p.paymentStatus === 2).length}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">Đang chờ:</span>
                    <span class="stat-value pending">${payments.filter(p => p.paymentStatus === 1).length}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">Thất bại:</span>
                    <span class="stat-value failed">${payments.filter(p => p.paymentStatus === 3).length}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">Tổng tiền:</span>
                    <span class="stat-value total">${this.formatCurrency(payments.filter(p => p.paymentStatus === 2).reduce((sum, p) => sum + p.amount, 0))}</span>
                </div>
            </div>
            
            <div class="table-container">
                <table class="detailed-payments-table" id="payments-table">
                    <thead>
                        <tr>
                            <th class="sortable" data-sort="payId">
                                Mã GD <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="paymentDate">
                                Ngày GD <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="patientName">
                                Bệnh nhân <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="serviceName">
                                Dịch vụ <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="amount">
                                Số tiền <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="paymentMethod">
                                Phương thức <i class="fas fa-sort"></i>
                            </th>
                            <th class="sortable" data-sort="paymentStatus">
                                Trạng thái <i class="fas fa-sort"></i>
                            </th>
                            <th>Mô tả</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody id="payments-tbody">
                        ${this.generatePaymentRows(sortedPayments)}
                    </tbody>
                </table>
            </div>
            
            <div class="pagination-container">
                <div class="pagination-info">
                    Hiển thị <span id="showing-start">1</span> - <span id="showing-end">${Math.min(50, sortedPayments.length)}</span> 
                    trong tổng số <span id="total-payments">${sortedPayments.length}</span> giao dịch
                </div>
                <div class="pagination-controls">
                    <button id="prev-page" class="page-btn" disabled><i class="fas fa-chevron-left"></i></button>
                    <span id="page-numbers"></span>
                    <button id="next-page" class="page-btn"><i class="fas fa-chevron-right"></i></button>
                </div>
            </div>
        `;
        
        detailedContainer.innerHTML = tableHtml;
        
        // Add event listeners
        this.setupPaymentControls();
        
        console.log(`✅ Updated detailed payments list with ${sortedPayments.length} payments`);
    }

    // Generate payment table rows
    generatePaymentRows(payments) {
        return payments.map(payment => {
            const statusClass = this.getPaymentStatusClass(payment.paymentStatus);
            const statusText = this.getPaymentStatusText(payment.paymentStatus);
            const methodText = this.formatPaymentMethod(payment.paymentMethod);
            const isCashPending = payment.paymentMethod === 'cash' && payment.paymentStatus === 1;
            
            return `
                <tr class="payment-row ${statusClass}" data-payment-id="${payment.payId}">
                    <td class="payment-id">
                        <strong>#${payment.payId}</strong>
                        ${payment.paymentIntentId ? `<br><small class="intent-id">${payment.paymentIntentId.slice(-8)}</small>` : ''}
                    </td>
                    <td class="payment-date">
                        <span class="date">${this.formatDate(payment.paymentDate)}</span>
                        <small class="time">${this.formatTimeOnly(payment.paymentDate)}</small>
                    </td>
                    <td class="patient-info">
                        <div class="patient-name">${payment.patientName}</div>
                        <small class="patient-email">${payment.patientEmail}</small>
                    </td>
                    <td class="service-info">
                        <div class="service-name">${payment.serviceName || 'N/A'}</div>
                        ${payment.servicePrice ? `<small class="service-price">Giá gốc: ${this.formatCurrency(payment.servicePrice)}</small>` : ''}
                    </td>
                    <td class="amount">
                        <span class="amount-value">${this.formatCurrency(payment.amount)}</span>
                        <small class="currency">${payment.currency}</small>
                    </td>
                    <td class="method">
                        <span class="method-badge method-${payment.paymentMethod?.toLowerCase()}">
                            <i class="fas fa-${this.getPaymentMethodIcon(payment.paymentMethod)}"></i>
                            ${methodText}
                        </span>
                    </td>
                    <td class="status">
                        <span class="status-badge status-${payment.paymentStatus}">
                            ${statusText}
                        </span>
                    </td>
                    <td class="description">
                        <div class="desc-text" title="${payment.description}">
                            ${payment.description ? (payment.description.length > 50 ? payment.description.substring(0, 50) + '...' : payment.description) : 'N/A'}
                        </div>
                    </td>
                    <td class="actions">
                        <div class="action-buttons">
                            <button class="view-btn" onclick="viewPaymentDetails('${payment.payId}')" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${isCashPending ? `
                                <button class="complete-btn" onclick="completePayment('${payment.payId}')" title="Hoàn thành">
                                    <i class="fas fa-check"></i>
                                </button>
                            ` : ''}
                            <button class="print-btn" onclick="printPaymentReceipt('${payment.payId}')" title="In hóa đơn">
                                <i class="fas fa-print"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');
    }

    // Create detailed payments section
    createDetailedPaymentsSection() {
        const mainContent = document.querySelector('.dashboard-grid') || document.querySelector('.main-content');
        if (!mainContent) return;
        
        const detailedSection = document.createElement('div');
        detailedSection.className = 'dashboard-card detailed-payments-section';
        detailedSection.innerHTML = `
            <div class="card-header">
                <h3><i class="fas fa-list-alt"></i> Chi tiết thanh toán</h3>
                <span class="card-subtitle">Quản lý và kiểm soát tất cả giao dịch</span>
            </div>
            <div class="card-content">
                <div id="detailed-payments-container"></div>
            </div>
        `;
        
        mainContent.appendChild(detailedSection);
    }

    // Setup payment controls and event listeners
    setupPaymentControls() {
        // Search functionality
        const searchInput = document.getElementById('payment-search');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.filterPayments();
            });
        }
        
        // Filter controls
        ['status-filter', 'method-filter', 'date-filter'].forEach(filterId => {
            const filter = document.getElementById(filterId);
            if (filter) {
                filter.addEventListener('change', () => {
                    this.filterPayments();
                });
            }
        });
        
        // Export button
        const exportBtn = document.getElementById('export-payments');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                this.exportPayments();
            });
        }
        
        // Refresh button
        const refreshBtn = document.getElementById('refresh-payments');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.loadPaymentData();
            });
        }
        
        // Table sorting
        document.querySelectorAll('.sortable').forEach(header => {
            header.addEventListener('click', (e) => {
                const sortField = e.currentTarget.dataset.sort;
                this.sortPayments(sortField);
            });
        });
    }

    // Filter payments based on search and filters
    filterPayments() {
        if (!this.paymentData) return;
        
        const searchTerm = document.getElementById('payment-search')?.value.toLowerCase() || '';
        const statusFilter = document.getElementById('status-filter')?.value || '';
        const methodFilter = document.getElementById('method-filter')?.value || '';
        const dateFilter = document.getElementById('date-filter')?.value || '';
        
        let filteredPayments = this.paymentData.filter(payment => {
            // Search filter
            const searchMatch = searchTerm === '' || 
                payment.patientName?.toLowerCase().includes(searchTerm) ||
                payment.serviceName?.toLowerCase().includes(searchTerm) ||
                payment.description?.toLowerCase().includes(searchTerm) ||
                payment.payId.toString().includes(searchTerm);
            
            // Status filter
            const statusMatch = statusFilter === '' || payment.paymentStatus.toString() === statusFilter;
            
            // Method filter
            const methodMatch = methodFilter === '' || payment.paymentMethod?.toLowerCase() === methodFilter;
            
            // Date filter
            let dateMatch = true;
            if (dateFilter) {
                const paymentDate = new Date(payment.paymentDate);
                const now = new Date();
                
                switch (dateFilter) {
                    case 'today':
                        dateMatch = paymentDate.toDateString() === now.toDateString();
                        break;
                    case 'week':
                        const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                        dateMatch = paymentDate >= weekAgo;
                        break;
                    case 'month':
                        dateMatch = paymentDate.getMonth() === now.getMonth() && 
                                   paymentDate.getFullYear() === now.getFullYear();
                        break;
                    case 'year':
                        dateMatch = paymentDate.getFullYear() === now.getFullYear();
                        break;
                }
            }
            
            return searchMatch && statusMatch && methodMatch && dateMatch;
        });
        
        // Update table
        const tbody = document.getElementById('payments-tbody');
        if (tbody) {
            tbody.innerHTML = this.generatePaymentRows(filteredPayments);
        }
        
        // Update stats
        this.updateFilteredStats(filteredPayments);
    }

    // Update stats after filtering
    updateFilteredStats(filteredPayments) {
        const totalEl = document.getElementById('total-payments');
        if (totalEl) totalEl.textContent = filteredPayments.length;
        
        const showingEnd = document.getElementById('showing-end');
        if (showingEnd) showingEnd.textContent = Math.min(50, filteredPayments.length);
    }

    // Sort payments by field
    sortPayments(field) {
        if (!this.paymentData) return;
        
        // Toggle sort direction
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        
        const sortedPayments = [...this.paymentData].sort((a, b) => {
            let aVal = a[field];
            let bVal = b[field];
            
            // Handle different data types
            if (field === 'paymentDate') {
                aVal = new Date(aVal);
                bVal = new Date(bVal);
            } else if (field === 'amount') {
                aVal = parseFloat(aVal) || 0;
                bVal = parseFloat(bVal) || 0;
            } else if (typeof aVal === 'string') {
                aVal = aVal.toLowerCase();
                bVal = bVal.toLowerCase();
            }
            
            if (this.sortDirection === 'asc') {
                return aVal > bVal ? 1 : -1;
            } else {
                return aVal < bVal ? 1 : -1;
            }
        });
        
        // Update table
        const tbody = document.getElementById('payments-tbody');
        if (tbody) {
            tbody.innerHTML = this.generatePaymentRows(sortedPayments);
        }
        
        // Update sort indicators
        document.querySelectorAll('.sortable i').forEach(icon => {
            icon.className = 'fas fa-sort';
        });
        
        const currentHeader = document.querySelector(`[data-sort="${field}"] i`);
        if (currentHeader) {
            currentHeader.className = `fas fa-sort-${this.sortDirection === 'asc' ? 'up' : 'down'}`;
        }
    }

    // Export payments to CSV/Excel format
    exportPayments() {
        if (!this.paymentData) return;
        
        const headers = ['Mã GD', 'Ngày', 'Bệnh nhân', 'Email', 'Dịch vụ', 'Số tiền', 'Tiền tệ', 'Phương thức', 'Trạng thái', 'Mô tả'];
        const csvData = [headers];
        
        this.paymentData.forEach(payment => {
            csvData.push([
                payment.payId,
                this.formatDate(payment.paymentDate),
                payment.patientName,
                payment.patientEmail,
                payment.serviceName || 'N/A',
                payment.amount,
                payment.currency,
                this.formatPaymentMethod(payment.paymentMethod),
                this.getPaymentStatusText(payment.paymentStatus),
                payment.description || 'N/A'
            ]);
        });
        
        const csvContent = csvData.map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');
        const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `payments_${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    // Format date for display
    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    // Format time only
    formatTimeOnly(dateString) {
        const date = new Date(dateString);
        return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    // Update payment method distribution chart
    updatePaymentMethodChart(payments) {
        const ctx = document.getElementById('paymentMethodChart');
        if (!ctx) {
            console.log('⚠️ Payment method chart canvas not found, creating one...');
            this.createPaymentMethodChart();
            return;
        }

        // Destroy existing chart if it exists
        if (this.charts.paymentMethod) {
            this.charts.paymentMethod.destroy();
        }

        // Calculate payment method distribution
        const methodCounts = {};
        payments.forEach(payment => {
            const method = this.formatPaymentMethod(payment.paymentMethod);
            methodCounts[method] = (methodCounts[method] || 0) + 1;
        });

        const labels = Object.keys(methodCounts);
        const data = Object.values(methodCounts);
        const colors = ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF'];

        this.charts.paymentMethod = new Chart(ctx, {
            type: 'pie',
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
                            padding: 15,
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
    }

    // Create payment method chart if it doesn't exist
    createPaymentMethodChart() {
        const chartsContainer = document.querySelector('.charts-container') || document.querySelector('.dashboard-grid');
        if (!chartsContainer) return;
        
        const chartContainer = document.createElement('div');
        chartContainer.className = 'dashboard-card chart-card';
        chartContainer.innerHTML = `
            <div class="card-header">
                <h3><i class="fas fa-chart-pie"></i> Payment Methods</h3>
            </div>
            <div class="card-content">
                <canvas id="paymentMethodChart"></canvas>
            </div>
        `;
        
        chartsContainer.appendChild(chartContainer);
    }

    // Update payment status chart
    updatePaymentStatusChart(payments) {
        const ctx = document.getElementById('paymentStatusChart');
        if (!ctx) {
            console.log('⚠️ Payment status chart canvas not found');
            return;
        }

        // Destroy existing chart if it exists
        if (this.charts.paymentStatus) {
            this.charts.paymentStatus.destroy();
        }

        // Calculate payment status distribution
        const statusCounts = { pending: 0, completed: 0, failed: 0 };
        payments.forEach(payment => {
            switch(payment.paymentStatus) {
                case 1: statusCounts.pending++; break;
                case 2: statusCounts.completed++; break;
                case 3: statusCounts.failed++; break;
            }
        });

        this.charts.paymentStatus = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Pending', 'Completed', 'Failed'],
                datasets: [{
                    data: [statusCounts.pending, statusCounts.completed, statusCounts.failed],
                    backgroundColor: ['#FFA726', '#66BB6A', '#EF5350'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Helper methods for payment display
    getPaymentStatusClass(status) {
        const classes = {
            1: 'payment-pending',
            2: 'payment-completed', 
            3: 'payment-failed'
        };
        return classes[status] || '';
    }

    getPaymentStatusText(status) {
        const texts = {
            1: 'Pending',
            2: 'Completed',
            3: 'Failed'
        };
        return texts[status] || 'Unknown';
    }

    formatPaymentMethod(method) {
        if (!method) return 'N/A';
        
        const methodMap = {
            'cash': 'Cash',
            'card': 'Credit Card',
            'transfer': 'Bank Transfer',
            'stripe': 'Stripe',
            'string': 'Other'
        };
        
        return methodMap[method.toLowerCase()] || method;
    }

    getPaymentMethodIcon(method) {
        if (!method) return 'question';
        
        const iconMap = {
            'cash': 'money-bill',
            'card': 'credit-card',
            'transfer': 'university',
            'stripe': 'stripe',
            'string': 'question'
        };
        
        return iconMap[method.toLowerCase()] || 'question';
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
    updateRevenueChart(data, paymentData = null) {
        const ctx = document.getElementById('revenueChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (this.charts.revenue) {
            this.charts.revenue.destroy();
        }

        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        let monthlyData;

        if (paymentData && paymentData.length > 0) {
            // Use actual payment data to calculate monthly revenue
            monthlyData = months.map((month, index) => {
                const currentYear = new Date().getFullYear();
                return paymentData
                    .filter(payment => {
                        const paymentDate = new Date(payment.paymentDate);
                        return payment.paymentStatus === 2 && // Only successful payments
                               paymentDate.getMonth() === index &&
                               paymentDate.getFullYear() === currentYear;
                    })
                    .reduce((sum, payment) => sum + payment.amount, 0);
            });
            
            console.log('📊 Using actual payment data for revenue chart:', monthlyData);
        } else {
            // Fallback to mock data
            const currentMonth = new Date().getMonth();
            monthlyData = months.map((month, index) => {
                if (index <= currentMonth) {
                    return index === currentMonth ? data.monthlyRevenue : Math.floor(Math.random() * 1000000) + 500000;
                }
                return 0;
            });
            
            console.log('📊 Using mock data for revenue chart');
        }

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

// Global functions for payment management
window.viewPaymentDetails = function(paymentId) {
    console.log('Viewing payment details for:', paymentId);
    // Implementation for viewing payment details
    alert(`Xem chi tiết thanh toán #${paymentId}`);
};

window.completePayment = async function(paymentId) {
    if (!confirm('Bạn có chắc chắn muốn đánh dấu thanh toán này là đã hoàn thành?')) {
        return;
    }
    
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/Payment/MarkCashPaymentSuccess/${paymentId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (response.ok) {
            alert('Thanh toán đã được hoàn thành thành công!');
            // Refresh the dashboard
            if (window.dashboardManager) {
                window.dashboardManager.loadPaymentData();
            }
        } else {
            alert('Có lỗi xảy ra khi hoàn thành thanh toán.');
        }
    } catch (error) {
        console.error('Error completing payment:', error);
        alert('Có lỗi xảy ra khi hoàn thành thanh toán.');
    }
};

window.printPaymentReceipt = function(paymentId) {
    console.log('Printing receipt for payment:', paymentId);
    // Implementation for printing receipt
    alert(`In hóa đơn cho thanh toán #${paymentId}`);
};
