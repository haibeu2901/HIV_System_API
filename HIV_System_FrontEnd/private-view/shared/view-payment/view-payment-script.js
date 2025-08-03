document.addEventListener('DOMContentLoaded', async function () {
    const section = document.getElementById('paymentsSection');

    // --- Filter Bar ---
    const filterBar = document.createElement('div');
    filterBar.className = 'filter-bar';
    filterBar.style = "margin-bottom:1rem;display:flex;flex-wrap:wrap;gap:1rem;align-items:center;";
    filterBar.innerHTML = `
        <label>Trạng thái:
            <select id="filterPaymentStatus">
                <option value="">Tất cả</option>
                <option value="1">Đang chờ</option>
                <option value="2">Đã thanh toán</option>
                <option value="3">Thất bại</option>
            </select>
        </label>
        <label>Ngày:
            <input type="date" id="filterPaymentDate">
        </label>
        <label>Bệnh nhân:
            <input type="text" id="filterPatientName" placeholder="Nhập tên bệnh nhân">
        </label>
        <label>Sắp xếp ngày:
            <select id="sortPaymentDate">
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ nhất</option>
            </select>
        </label>
        <button id="clearPaymentFilters" type="button">Xóa lọc</button>
    `;
    section.parentNode.insertBefore(filterBar, section);

    section.innerHTML = '<p>Đang tải dữ liệu...</p>';

    let allPayments = [];
    const token = localStorage.getItem('token');
    try {
        const res = await fetch('https://localhost:7009/api/Payment/GetAllPayments', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Lỗi khi tải danh sách thanh toán');
        allPayments = await res.json();
        applyPaymentFilters();
    } catch (err) {
        section.innerHTML = `<div style="color:red;">${err.message}</div>`;
    }

    function applyPaymentFilters() {
        const status = document.getElementById('filterPaymentStatus').value;
        const date = document.getElementById('filterPaymentDate').value;
        const patientName = document.getElementById('filterPatientName').value.trim().toLowerCase();
        const sortOrder = document.getElementById('sortPaymentDate').value;

        let filtered = allPayments.filter(p => {
            let match = true;
            if (status && String(p.paymentStatus) !== status) match = false;
            if (date && (!p.paymentDate || p.paymentDate.slice(0, 10) !== date)) match = false;
            if (patientName && !(p.patientName || '').toLowerCase().includes(patientName)) match = false;
            return match;
        });

        filtered.sort((a, b) => {
            const dateA = new Date(a.paymentDate);
            const dateB = new Date(b.paymentDate);
            if (sortOrder === 'asc') return dateA - dateB;
            return dateB - dateA;
        });

        renderPayments(filtered);
    }

    function renderPayments(payments) {
        if (!payments || payments.length === 0) {
            section.innerHTML = `<div class="empty-state"><i class="fas fa-credit-card"></i> Không có thanh toán nào.</div>`;
            return;
        }
        section.innerHTML = `
            <table class="payments-table">
                <thead>
                    <tr>
                        <th>Mã thanh toán</th>
                        <th>Bệnh nhân</th>
                        <th>Số tiền</th>
                        <th>Phương thức</th>
                        <th>Trạng thái</th>
                        <th>Dịch vụ</th>
                        <th>Ngày thanh toán</th>
                        <th>Mô tả</th>
                    </tr>
                </thead>
                <tbody>
                    ${payments.map(p => `
                        <tr>
                            <td>${p.payId}</td>
                            <td>${p.patientName || '-'}</td>
                            <td>${Number(p.amount).toLocaleString('vi-VN')} ${p.currency ? p.currency.toUpperCase() : ''}</td>
                            <td>${formatPaymentMethod(p.paymentMethod)}</td>
                            <td class="payment-status-${p.paymentStatus}">${formatPaymentStatus(p.paymentStatus)}</td>
                            <td>${p.serviceName || '-'}</td>
                            <td>${formatDateTime(p.paymentDate)}</td>
                            <td>${p.description || '-'}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;
    }

    function formatPaymentStatus(status) {
        switch (status) {
            case 1: return 'Đang chờ';
            case 2: return 'Đã thanh toán';
            case 3: return 'Thất bại';
            default: return status;
        }
    }
    function formatPaymentMethod(method) {
        if (!method) return '-';
        const m = method.toLowerCase();
        if (m.includes('cash')) return 'Tiền mặt';
        if (m.includes('stripe')) return 'Stripe';
        if (m.includes('bank')) return 'Chuyển khoản';
        return method;
    }
    function formatDateTime(dt) {
        if (!dt) return '-';
        const d = new Date(dt);
        if (isNaN(d)) return dt;
        return d.toLocaleString('vi-VN');
    }

    // Filter event listeners
    document.getElementById('filterPaymentStatus').addEventListener('change', applyPaymentFilters);
    document.getElementById('filterPaymentDate').addEventListener('change', applyPaymentFilters);
    document.getElementById('filterPatientName').addEventListener('input', applyPaymentFilters);
    document.getElementById('sortPaymentDate').addEventListener('change', applyPaymentFilters);
    document.getElementById('clearPaymentFilters').addEventListener('click', function () {
        document.getElementById('filterPaymentStatus').value = '';
        document.getElementById('filterPaymentDate').value = '';
        document.getElementById('filterPatientName').value = '';
        document.getElementById('sortPaymentDate').value = 'desc';
        applyPaymentFilters();
    });
});