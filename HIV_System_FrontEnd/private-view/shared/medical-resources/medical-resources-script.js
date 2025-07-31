// Get token from localStorage
const token = localStorage.getItem('token');

async function fetchArvMedicines() {
    try {
        const response = await fetch('https://localhost:7009/api/ArvMedicationDetail/GetAllArvMedicationDetails', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch ARV medicines');
        return await response.json();
    } catch (error) {
        console.error('Error fetching ARV medicines:', error);
        return [];
    }
}

function formatPrice(price) {
    return price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
}

function renderArvMedicines(medicines) {
    const section = document.getElementById('medicalResourcesSection');
    if (!medicines.length) {
        section.innerHTML = '<p>Không có dữ liệu thuốc ARV.</p>';
        return;
    }
    let html = `<table class="arv-table">
        <thead>
            <tr>
                <th>Tên thuốc</th>
                <th>Mô tả</th>
                <th>Liều dùng</th>
                <th>Giá</th>
                <th>Nhà sản xuất</th>
                <th>Loại</th>
            </tr>
        </thead>
        <tbody>
            ${medicines.map(med => `
                <tr>
                    <td>${med.arvMedicationName}</td>
                    <td>${med.arvMedicationDescription}</td>
                    <td>${med.arvMedicationDosage}</td>
                    <td>${formatPrice(med.arvMedicationPrice)}</td>
                    <td>${med.arvMedicationManufacturer}</td>
                    <td>${med.arvMedicationType}</td>
                </tr>
            `).join('')}
        </tbody>
    </table>`;
    section.innerHTML = html;
}

window.addEventListener('DOMContentLoaded', async () => {
    const medicines = await fetchArvMedicines();
    renderArvMedicines(medicines);
});
