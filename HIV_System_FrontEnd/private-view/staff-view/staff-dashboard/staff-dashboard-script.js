// Staff Dashboard Script
async function fetchStaffDashboard(accId) {
    const token = localStorage.getItem('token');
    try {
        const response = await fetch(`https://localhost:7009/api/Dashboard/staff/${accId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Failed to fetch staff dashboard');
        return await response.json();
    } catch (error) {
        console.error('Error fetching staff dashboard:', error);
        return null;
    }
}

function renderDashboardCards(dashboard) {
    return `
        <div class="dashboard-cards" style="display: flex; gap: 2rem; flex-wrap: wrap; margin-bottom:2rem;">
            <div class="dashboard-card" style="flex:1; min-width:200px; background:#fff; border-radius:12px; box-shadow:0 2px 8px #0001; padding:2rem; text-align:center;">
                <i class="fas fa-vial" style="font-size:2rem; color:#e74c3c;"></i>
                <h3 style="margin:1rem 0 0.5rem;">Xét nghiệm hôm nay</h3>
                <div style="font-size:2rem; font-weight:bold;">${dashboard.todayTestResults ?? '0'}</div>
            </div>
            <div class="dashboard-card" style="flex:1; min-width:200px; background:#fff; border-radius:12px; box-shadow:0 2px 8px #0001; padding:2rem; text-align:center;">
                <i class="fas fa-calendar-alt" style="font-size:2rem; color:#3498db;"></i>
                <h3 style="margin:1rem 0 0.5rem;">Xét nghiệm tháng này</h3>
                <div style="font-size:2rem; font-weight:bold;">${dashboard.monthlyTestResults ?? '0'}</div>
            </div>
            <div class="dashboard-card" style="flex:1; min-width:200px; background:#fff; border-radius:12px; box-shadow:0 2px 8px #0001; padding:2rem; text-align:center;">
                <i class="fas fa-database" style="font-size:2rem; color:#27ae60;"></i>
                <h3 style="margin:1rem 0 0.5rem;">Tổng số xét nghiệm</h3>
                <div style="font-size:2rem; font-weight:bold;">${dashboard.totalTestResults ?? '0'}</div>
            </div>
            <div class="dashboard-card" style="flex:1; min-width:200px; background:#fff; border-radius:12px; box-shadow:0 2px 8px #0001; padding:2rem; text-align:center;">
                <i class="fas fa-hourglass-half" style="font-size:2rem; color:#f39c12;"></i>
                <h3 style="margin:1rem 0 0.5rem;">Chờ xử lý</h3>
                <div style="font-size:2rem; font-weight:bold;">${dashboard.pendingTests ?? '0'}</div>
            </div>
        </div>
    `;
}

function renderWorkloadSummary(workload) {
    if (!workload || !workload.length) return '';
    return `
        <div class="dashboard-section" style="margin-bottom:2rem;">
            <h3 style="margin-bottom:1rem;">Tổng kết khối lượng công việc</h3>
            <table style="width:100%; border-collapse:collapse;">
                <thead>
                    <tr style="background:#f8f8f8;">
                        <th style="padding:8px; border-bottom:1px solid #eee; text-align:left;">Tên xét nghiệm</th>
                        <th style="padding:8px; border-bottom:1px solid #eee; text-align:right;">Số lượng</th>
                    </tr>
                </thead>
                <tbody>
                    ${workload.map(item => `
                        <tr>
                            <td style="padding:8px;">${item.testName}</td>
                            <td style="padding:8px; text-align:right;">${item.count}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
}

function renderRecentTestResults(recent) {
    if (!recent || !recent.length) return '';
    return `
        <div class="dashboard-section">
            <h3 style="margin-bottom:1rem;">Kết quả xét nghiệm gần đây</h3>
            <div style="display:flex; flex-direction:column; gap:1rem;">
                ${recent.map(r => `
                    <div style="background:#fafafa; border-radius:8px; box-shadow:0 1px 4px #0001; padding:1rem 1.5rem;">
                        <div style="font-weight:bold; color:#e74c3c;">${r.ctrName} (${r.testDate})</div>
                        <div><strong>Kết quả:</strong> ${r.resultValue}</div>
                        <div><strong>Ghi chú:</strong> ${r.notes}</div>
                    </div>
                `).join('')}
            </div>
        </div>
    `;
}

async function renderStaffDashboard(accId, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = '<p>Đang tải dữ liệu bảng điều khiển...</p>';
    const dashboard = await fetchStaffDashboard(accId);
    if (dashboard) {
        container.innerHTML = `
            <div class="dashboard-header">
                <h2>Bảng điều khiển nhân viên</h2>
            </div>
            ${renderDashboardCards(dashboard)}
            ${renderWorkloadSummary(dashboard.workloadSummary)}
            ${renderRecentTestResults(dashboard.recentTestResults)}
        `;
    } else {
        container.innerHTML = '<p>Không thể tải dữ liệu bảng điều khiển.</p>';
    }
}

window.addEventListener('DOMContentLoaded', () => {
    const accId = localStorage.getItem('accId');
    renderStaffDashboard(accId, 'dashboardSection');
});
