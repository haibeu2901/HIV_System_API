// Existing mappings
const regimenStatusMap = {
    1: "Đã lên kế hoạch",
    2: "Đang hoạt động",
    3: "Tạm dừng",
    4: "Đã hủy",
    5: "Hoàn thành"
};

// Assuming regimenLevelMap exists (based on previous context)
const regimenLevelMap = {
    1: "Bậc 1",
    2: "Bậc 2",
    3: "Bậc 3"
    // Add other levels as needed
};

// Utility to format date for display
function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Utility to normalize date for filtering
function normalizeDate(dateStr) {
    if (!dateStr) return '';
    return dateStr.slice(0, 10); // Assumes YYYY-MM-DD format
}

// Utility to normalize time for filtering (from appointment-list-script.js)
function normalizeTime(timeStr) {
    if (!timeStr) return '';
    const date = new Date(timeStr);
    return date.toISOString().slice(11, 16); // Returns HH:MM
}

// Render Patient Profile (unchanged)
function renderPatientProfile(patient) {
    const section = document.getElementById('patientProfileSection');
    if (!section) return;
    section.innerHTML = `
        <div class="patient-profile">
            <h3>Hồ sơ bệnh nhân</h3>
            <p><strong>Họ và tên:</strong> ${patient.fullName || 'N/A'}</p>
            <p><strong>Mã bệnh nhân:</strong> ${patient.patientCode || 'N/A'}</p>
            <p><strong>Ngày sinh:</strong> ${patient.dob ? new Date(patient.dob).toLocaleDateString('vi-VN') : 'N/A'}</p>
            <p><strong>Giới tính:</strong> ${patient.gender || 'N/A'}</p>
            <p><strong>Số điện thoại:</strong> ${patient.phone || 'N/A'}</p>
            <p><strong>Địa chỉ:</strong> ${patient.address || 'N/A'}</p>
        </div>
    `;
}

// Render Appointments (unchanged)
function renderAppointments(appointments) {
    const section = document.getElementById('appointmentsSection');
    if (!section) return;
    if (!appointments || !appointments.length) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-calendar-times"></i>
                <p>Không tìm thấy lịch hẹn nào.</p>
            </div>
        `;
        return;
    }
    let html = `
        <table class="appointments-table" style="width:100%;border-collapse:collapse;">
            <thead>
                <tr>
                    <th>Ngày hẹn</th>
                    <th>Giờ hẹn</th>
                    <th>Trạng thái</th>
                    <th>Ghi chú</th>
                    <th>Ngày tạo</th>
                    <th>Chi tiết</th>
                </tr>
            </thead>
            <tbody>
    `;
    appointments.forEach((appointment, idx) => {
        html += `
            <tr class="appointment-row" data-idx="${idx}">
                <td>${appointment.apmtDate || 'N/A'}</td>
                <td>${appointment.apmTime || 'N/A'}</td>
                <td><span class="appointment-status status-${appointment.apmStatus}">${appointment.apmStatus === 1 ? 'Chờ xác nhận' : appointment.apmStatus === 2 ? 'Đã xác nhận' : appointment.apmStatus === 3 ? 'Đã hủy' : appointment.apmStatus === 4 ? 'Hoàn thành' : 'Không xác định'}</span></td>
                <td>${appointment.notes || ''}</td>
                <td>${formatDateTime(appointment.createdAt)}</td>
                <td>
                    <button class="toggle-details-btn" data-idx="${idx}">▼</button>
                </td>
            </tr>
            <tr class="appointment-details-row" id="appointment-details-${idx}" style="display:none;">
                <td colspan="6">
                    <div><strong>Ngày tạo:</strong> ${formatDateTime(appointment.createdAt)}</div>
                    <div><strong>Ghi chú chi tiết:</strong> ${appointment.notes || 'Không có'}</div>
                    ${window.isStaff ? `
                        <div style="margin-top:1rem;text-align:right;">
                            <button class="secondary-btn update-appointment-btn" data-id="${appointment.appointmentId}">Cập nhật</button>
                        </div>
                    ` : ''}
                </td>
            </tr>
        `;
    });
    html += `</tbody></table>`;
    section.innerHTML = html;

    document.querySelectorAll('.toggle-details-btn').forEach(btn => {
        btn.onclick = function(e) {
            e.stopPropagation();
            const idx = this.getAttribute('data-idx');
            const detailsRow = document.getElementById(`appointment-details-${idx}`);
            if (detailsRow.style.display === 'none') {
                detailsRow.style.display = '';
                this.textContent = '▲';
            } else {
                detailsRow.style.display = 'none';
                this.textContent = '▼';
            }
        };
    });

    if (window.isStaff) {
        section.querySelectorAll('.update-appointment-btn').forEach(btn => {
            btn.addEventListener('click', async function () {
                const appointmentId = this.getAttribute('data-id');
                if (!appointmentId) return;
                const appointment = appointments.find(apm => String(apm.appointmentId) === String(appointmentId));
                if (!appointment) {
                    alert('Không tìm thấy dữ liệu lịch hẹn.');
                    return;
                }
                document.getElementById('updateAppointmentId').value = appointment.appointmentId;
                document.getElementById('updateAppointmentDate').value = appointment.apmtDate || '';
                document.getElementById('updateAppointmentTime').value = appointment.apmTime || '';
                document.getElementById('updateAppointmentStatus').value = appointment.apmStatus || '';
                document.getElementById('updateAppointmentNotes').value = appointment.notes || '';
                document.getElementById('updateAppointmentModal').style.display = 'block';
            });
        });
    }
}

// Render ARV Regimens with Filter Bar
function renderARVRegimens(regimens, medications) {
    const section = document.getElementById('arvRegimensSection');
    if (!section) return;

    let allRegimens = regimens || [];

    // Add filter bar
    if (!document.getElementById('regimenFilterStatus')) {
        const filterBar = document.createElement('div');
        filterBar.className = 'filter-bar';
        filterBar.innerHTML = `
            <label for="regimenFilterStatus">Trạng thái:</label>
            <select id="regimenFilterStatus">
                <option value="">Tất cả</option>
                <option value="1">Đã lên kế hoạch</option>
                <option value="2">Đang hoạt động</option>
                <option value="3">Tạm dừng</option>
                <option value="4">Đã hủy</option>
                <option value="5">Hoàn thành</option>
            </select>
            <label for="regimenFilterDate">Ngày tạo:</label>
            <input type="date" id="regimenFilterDate">
            <label for="regimenFilterTime">Giờ:</label>
            <input type="time" id="regimenFilterTime">
            <label for="regimenSortDate">Sắp xếp ngày tạo:</label>
            <select id="regimenSortDate">
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ nhất</option>
            </select>
            <button id="regimenClearFilters">Xóa lọc</button>
        `;
        section.parentNode.insertBefore(filterBar, section);
    }

    function applyRegimenFilters() {
        const status = document.getElementById('regimenFilterStatus').value;
        const date = document.getElementById('regimenFilterDate').value;
        const time = document.getElementById('regimenFilterTime').value;
        const sortOrder = document.getElementById('regimenSortDate').value;

        let filtered = allRegimens.filter(regimen => {
            let match = true;
            if (status && String(regimen.regimenStatus) !== status) match = false;
            if (date && normalizeDate(regimen.createdAt) !== date) match = false;
            if (time && normalizeTime(regimen.createdAt) !== time) match = false;
            return match;
        });

        filtered.sort((a, b) => {
            const dateA = new Date(a.createdAt || '1970-01-01');
            const dateB = new Date(b.createdAt || '1970-01-01');
            return sortOrder === 'asc' ? dateA - dateB : dateB - dateA;
        });

        renderFilteredRegimens(filtered);
    }

    function renderFilteredRegimens(regimens) {
        if (!regimens.length) {
            section.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-pills"></i>
                    <p>Không tìm thấy phác đồ ARV nào.</p>
                </div>
            `;
            return;
        }

        let html = `
            <table class="regimens-table" style="width:100%;border-collapse:collapse;">
                <thead>
                    <tr>
                        <th>Bậc</th>
                        <th>Trạng thái</th>
                        <th>Ngày bắt đầu</th>
                        <th>Ngày kết thúc</th>
                        <th>Ghi chú</th>
                        <th>Ngày tạo</th>
                        <th>Mở rộng/Đóng</th>
                    </tr>
                </thead>
                <tbody>
        `;
        regimens.forEach((regimen, idx) => {
            const statusText = regimenStatusMap[regimen.regimenStatus] || regimen.regimenStatus;
            const levelText = regimenLevelMap[regimen.regimenLevel] || regimen.regimenLevel;
            const regimenMeds = (medications || []).filter(med => med.patientArvRegiId === regimen.patientArvRegiId);
            html += `
                <tr class="regimen-row" data-idx="${idx}">
                    <td>${levelText}</td>
                    <td><span class="regimen-status status-${regimen.regimenStatus}">${statusText}</span></td>
                    <td>${regimen.startDate}</td>
                    <td>${regimen.endDate || 'Đang áp dụng'}</td>
                    <td>${regimen.notes || ''}</td>
                    <td>${formatDateTime(regimen.createdAt)}</td>
                    <td>
                        <button class="toggle-details-btn" data-idx="${idx}">▼</button>
                    </td>
                </tr>
                <tr class="regimen-details-row" id="regimen-details-${idx}" style="display:none;">
                    <td colspan="7">
                        <div><strong>Ngày tạo:</strong> ${formatDateTime(regimen.createdAt)}</div>
                        <div><strong>Tổng chi phí:</strong> ${regimen.totalCost ? regimen.totalCost.toLocaleString('vi-VN') + ' VND' : 'Không xác định'}</div>
                        <h4>Thuốc</h4>
                        ${regimenMeds.length > 0 ? `
                            <table class="medications-table" style="width:100%;margin-top:10px;">
                                <thead>
                                    <tr>
                                        <th>Tên thuốc</th>
                                        <th>Loại thuốc</th>
                                        <th>Liều lượng</th>
                                        <th>Số lượng</th>
                                        <th>Nhà sản xuất</th>
                                        <th>Cách sử dụng</th>
                                        <th>Giá</th>
                                        <th>Mô tả</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${regimenMeds.map(med => `
                                        <tr>
                                            <td>${med.medicationDetail.arvMedicationName}</td>
                                            <td>${med.medicationDetail.arvMedicationType || med.medicationDetail.medicationType || ''}</td>
                                            <td>${med.medicationDetail.arvMedicationDosage}</td>
                                            <td>${med.quantity}</td>
                                            <td>${med.medicationDetail.arvMedicationManufacturer}</td>
                                            <td>${med.usageInstructions || med.medicationDetail.medicationUsage || ''}</td>
                                            <td>${med.medicationDetail.arvMedicationPrice ? med.medicationDetail.arvMedicationPrice.toLocaleString() : 'N/A'}</td>
                                            <td>${med.medicationDetail.arvMedicationDescription}</td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        ` : `<div class='empty-state'><i class='fas fa-capsules'></i> Không có thuốc cho phác đồ này.</div>`}
                        <div style="margin-top:1rem;text-align:right;">
                            ${(!window.isStaff && (regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5)) ? `<button class="secondary-btn update-regimen-status-btn" data-id="${regimen.patientArvRegiId}" data-status="${regimen.regimenStatus}">Cập nhật trạng thái</button>` : ''}
                            ${(window.isDoctor && (regimen.regimenStatus !== 4 && regimen.regimenStatus !== 5)) ? `<button class="secondary-btn update-regimen-btn" data-id="${regimen.patientArvRegiId}">Cập nhật phác đồ</button>` : ''}
                        </div>
                    </td>
                </tr>
            `;
        });
        html += `</tbody></table>`;
        section.innerHTML = html;

        document.querySelectorAll('.toggle-details-btn').forEach(btn => {
            btn.onclick = function(e) {
                e.stopPropagation();
                const idx = this.getAttribute('data-idx');
                const detailsRow = document.getElementById(`regimen-details-${idx}`);
                if (detailsRow.style.display === 'none') {
                    detailsRow.style.display = '';
                    this.textContent = '▲';
                } else {
                    detailsRow.style.display = 'none';
                    this.textContent = '▼';
                }
            };
        });

        if (window.isStaff) {
            section.querySelectorAll('.update-regimen-btn').forEach(btn => {
                btn.addEventListener('click', async function () {
                    const regimenId = this.getAttribute('data-id');
                    if (!regimenId) return;
                    const regimen = regimens.find(tr => String(tr.patientArvRegiId) === String(regimenId));
                    if (!regimen) {
                        alert('Không tìm thấy dữ liệu phác đồ.');
                        return;
                    }
                    await loadMedicationDetails();
                    regimenModal.style.display = 'block';
                    const modalTitle = regimenModal.querySelector('h2');
                    if (modalTitle) modalTitle.textContent = 'Cập nhật phác đồ ARV';
                    const updateRegimenTemplate = document.getElementById('regimenTemplate');
                    if (updateRegimenTemplate && updateRegimenTemplate.parentElement) {
                        updateRegimenTemplate.parentElement.style.display = 'none';
                        updateRegimenTemplate.removeAttribute('required');
                        updateRegimenTemplate.value = '';
                    }
                    regimenLevel.value = regimen.regimenLevel;
                    regimenNotes.value = regimen.notes || '';
                    regimenStartDate.value = regimen.startDate;
                    if (document.getElementById('regimenEndDate')) {
                        document.getElementById('regimenEndDate').value = regimen.endDate || '';
                    }
                    selectedTemplateMedications = (regimen.arvMedications || []).map(med => ({
                        arvMedicationName: med.medicationDetail.arvMedicationName,
                        arvMedDetailId: med.medicationDetail.arvMedicationId,
                        dosage: med.medicationDetail.arvMedicationDosage,
                        quantity: med.quantity,
                        manufacturer: med.medicationDetail.arvMedicationManufacturer,
                        usageInstructions: (typeof med.usageInstructions === 'string' && med.usageInstructions.length > 0) ? med.usageInstructions : (med.medicationDetail.medicationUsage || '')
                    }));
                    renderMedicationRows();
                    regimenForm.setAttribute('data-update-id', regimenId);
                    const submitBtn = regimenForm.querySelector('button[type="submit"]');
                    if (submitBtn) submitBtn.textContent = 'Cập nhật';
                });
            });
            section.querySelectorAll('.update-regimen-status-btn').forEach(btn => {
                btn.addEventListener('click', async function () {
                    const regimenId = this.getAttribute('data-id');
                    const regimen = regimens.find(tr => String(tr.patientArvRegiId) === String(regimenId));
                    if (!regimen) {
                        alert('Không tìm thấy dữ liệu phác đồ.');
                        return;
                    }
                    const modal = document.getElementById('updateRegimenStatusModal');
                    const select = document.getElementById('updateRegimenStatusSelect');
                    const idInput = document.getElementById('updateRegimenStatusId');
                    const msg = document.getElementById('updateRegimenStatusMsg');
                    select.innerHTML = `
                        <option value="">Chọn trạng thái</option>
                        <option value="2" ${regimen.regimenStatus === 2 ? 'selected' : ''}>Đang hoạt động</option>
                        <option value="3" ${regimen.regimenStatus === 3 ? 'selected' : ''}>Tạm dừng</option>
                        <option value="4" ${regimen.regimenStatus === 4 ? 'selected' : ''}>Đã hủy</option>
                        <option value="5" ${regimen.regimenStatus === 5 ? 'selected' : ''}>Hoàn thành</option>
                    `;
                    idInput.value = regimenId;
                    msg.textContent = '';
                    modal.style.display = 'block';
                });
            });
        }
        window._lastRegimens = regimens;
    }

    applyRegimenFilters();

    document.getElementById('regimenFilterStatus').addEventListener('change', applyRegimenFilters);
    document.getElementById('regimenFilterDate').addEventListener('change', applyRegimenFilters);
    document.getElementById('regimenFilterTime').addEventListener('change', applyRegimenFilters);
    document.getElementById('regimenSortDate').addEventListener('change', applyRegimenFilters);
    document.getElementById('regimenClearFilters').addEventListener('click', function () {
        document.getElementById('regimenFilterStatus').value = '';
        document.getElementById('regimenFilterDate').value = '';
        document.getElementById('regimenFilterTime').value = '';
        document.getElementById('regimenSortDate').value = 'desc';
        applyRegimenFilters();
    });
}

// Render Test Results with Filter Bar
function renderTestResults(testResults) {
    const section = document.getElementById('testResultsSection');
    if (!section) return;

    let allTestResults = testResults || [];

    // Add filter bar
    if (!document.getElementById('testResultFilterResult')) {
        const filterBar = document.createElement('div');
        filterBar.className = 'filter-bar';
        filterBar.innerHTML = `
            <label for="testResultFilterResult">Kết quả:</label>
            <select id="testResultFilterResult">
                <option value="">Tất cả</option>
                <option value="true">Dương tính</option>
                <option value="false">Âm tính</option>
            </select>
            <label for="testResultFilterDate">Ngày xét nghiệm:</label>
            <input type="date" id="testResultFilterDate">
            <label for="testResultFilterTime">Giờ:</label>
            <input type="time" id="testResultFilterTime">
            <label for="testResultSortDate">Sắp xếp ngày tạo:</label>
            <select id="testResultSortDate">
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ nhất</option>
            </select>
            <button id="testResultClearFilters">Xóa lọc</button>
        `;
        section.parentNode.insertBefore(filterBar, section);
    }

    function applyTestResultFilters() {
        const result = document.getElementById('testResultFilterResult').value;
        const date = document.getElementById('testResultFilterDate').value;
        const time = document.getElementById('testResultFilterTime').value;
        const sortOrder = document.getElementById('testResultSortDate').value;

        let filtered = allTestResults.filter(testResult => {
            let match = true;
            if (result && String(testResult.result) !== result) match = false;
            if (date && normalizeDate(testResult.testDate) !== date) match = false;
            if (time && normalizeTime(testResult.testDate) !== time) match = false;
            return match;
        });

        filtered.sort((a, b) => {
            const dateA = new Date(a.createdAt || '1970-01-01');
            const dateB = new Date(b.createdAt || '1970-01-01');
            return sortOrder === 'asc' ? dateA - dateB : dateB - dateA;
        });

        renderFilteredTestResults(filtered);
    }

    function renderFilteredTestResults(testResults) {
        if (!testResults.length) {
            section.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-flask"></i>
                    <p>Không tìm thấy kết quả xét nghiệm nào.</p>
                </div>
            `;
            return;
        }
        let html = `
            <table class="test-results-table" style="width:100%;border-collapse:collapse;">
                <thead>
                    <tr>
                        <th>Ngày xét nghiệm</th>
                        <th>Kết quả</th>
                        <th>Ghi chú</th>
                        <th>Ngày tạo</th>
                        <th>Mở rộng/Đóng</th>
                    </tr>
                </thead>
                <tbody>
        `;
        testResults.forEach((testResult, idx) => {
            const resultClass = testResult.result ? 'test-result-positive' : 'test-result-negative';
            const resultText = testResult.result ? 'Dương tính' : 'Âm tính';
            html += `
                <tr class="test-result-row" data-idx="${idx}">
                    <td>${testResult.testDate}</td>
                    <td><span class="test-result-overall ${resultClass}">${resultText}</span></td>
                    <td>${testResult.notes || ''}</td>
                    <td>${formatDateTime(testResult.createdAt)}</td>
                    <td>
                        <button class="toggle-test-details-btn" data-idx="${idx}">▼</button>
                    </td>
                </tr>
                <tr class="test-result-details-row" id="test-result-details-${idx}" style="display:none;">
                    <td colspan="5">
                        <div><strong>Ngày tạo:</strong> ${formatDateTime(testResult.createdAt)}</div>
                        <div><strong>Thành phần xét nghiệm:</strong></div>
                        ${testResult.componentTestResults && testResult.componentTestResults.length > 0 ? `
                            <table class="medications-table" style="width:100%;margin-top:10px;">
                                <thead>
                                    <tr>
                                        <th>Tên thành phần</th>
                                        <th>Mô tả</th>
                                        <th>Giá trị</th>
                                        <th>Ghi chú</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${testResult.componentTestResults.map(comp => `
                                        <tr>
                                            <td>${comp.componentTestResultName || ''}</td>
                                            <td>${comp.ctrDescription || ''}</td>
                                            <td>${comp.resultValue || ''}</td>
                                            <td>${comp.notes || ''}</td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        ` : `<div class='empty-state'><i class='fas fa-capsules'></i> Không có thành phần xét nghiệm.</div>`}
                        <div style="margin-top:1rem;text-align:right;">
                            ${window.isStaff ? `<button class="secondary-btn update-test-result-btn" data-id="${testResult.testResultId}">Cập nhật kết quả</button>` : ''}
                        </div>
                    </td>
                </tr>
            `;
        });
        html += `</tbody></table>`;
        section.innerHTML = html;

        document.querySelectorAll('.toggle-test-details-btn').forEach(btn => {
            btn.onclick = function(e) {
                e.stopPropagation();
                const idx = this.getAttribute('data-idx');
                const detailsRow = document.getElementById(`test-result-details-${idx}`);
                if (detailsRow.style.display === 'none') {
                    detailsRow.style.display = '';
                    this.textContent = '▲';
                } else {
                    detailsRow.style.display = 'none';
                    this.textContent = '▼';
                }
            };
        });

        if (window.isStaff) {
            section.querySelectorAll('.update-test-result-btn').forEach(btn => {
                btn.addEventListener('click', async function () {
                    const testResultId = this.getAttribute('data-id');
                    if (!testResultId) return;
                    const testResult = testResults.find(tr => String(tr.testResultId) === String(testResultId));
                    if (!testResult) {
                        alert('Không tìm thấy dữ liệu kết quả xét nghiệm.');
                        return;
                    }
                    document.getElementById('updateTestResultId').value = testResult.testResultId;
                    document.getElementById('updateTestResultDate').value = testResult.testDate || '';
                    document.getElementById('updateTestResultSelect').value = testResult.result ? 'true' : 'false';
                    document.getElementById('updateTestResultNotes').value = testResult.notes || '';
                    loadComponentTestResultsFromData(testResult.componentTestResults || []);
                    document.getElementById('updateTestResultModal').style.display = 'block';
                });
            });
        }
    }

    applyTestResultFilters();

    document.getElementById('testResultFilterResult').addEventListener('change', applyTestResultFilters);
    document.getElementById('testResultFilterDate').addEventListener('change', applyTestResultFilters);
    document.getElementById('testResultFilterTime').addEventListener('change', applyTestResultFilters);
    document.getElementById('testResultSortDate').addEventListener('change', applyTestResultFilters);
    document.getElementById('testResultClearFilters').addEventListener('click', function () {
        document.getElementById('testResultFilterResult').value = '';
        document.getElementById('testResultFilterDate').value = '';
        document.getElementById('testResultFilterTime').value = '';
        document.getElementById('testResultSortDate').value = 'desc';
        applyTestResultFilters();
    });
}

// Render Payments (unchanged)
function renderPayments(payments) {
    const section = document.getElementById('paymentsSection');
    if (!section) return;
    if (!payments || !payments.length) {
        section.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-money-bill-wave"></i>
                <p>Không tìm thấy thanh toán nào.</p>
            </div>
        `;
        return;
    }
    let html = `
        <table class="payments-table" style="width:100%;border-collapse:collapse;">
            <thead>
                <tr>
                    <th>Ngày thanh toán</th>
                    <th>Số tiền</th>
                    <th>Phương thức</th>
                    <th>Ghi chú</th>
                    <th>Ngày tạo</th>
                </tr>
            </thead>
            <tbody>
    `;
    payments.forEach(payment => {
        html += `
            <tr>
                <td>${payment.paymentDate || 'N/A'}</td>
                <td>${payment.amount ? payment.amount.toLocaleString('vi-VN') + ' VND' : 'N/A'}</td>
                <td>${payment.paymentMethod || 'N/A'}</td>
                <td>${payment.notes || ''}</td>
                <td>${formatDateTime(payment.createdAt)}</td>
            </tr>
        `;
    });
    html += `</tbody></table>`;
    section.innerHTML = html;
}

// Load Patient Data (unchanged)
async function loadPatientData() {
    const patientId = getPatientIdFromUrl();
    if (!patientId) {
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
                <p>No patient ID provided in URL.</p>
            </div>
        `;
        return;
    }

    try {
        const patient = await fetchPatientDetails(patientId);
        renderPatientProfile(patient);
        const medicalData = await fetchPatientMedicalDataByPatientId(patientId);
        if (medicalData && medicalData.pmrId != null) {
            window.pmrId = medicalData.pmrId;
        } else {
            window.pmrId = null;
        }
        let medications = medicalData?.arvRegimens?.flatMap(r => r.arvMedications || []) || [];
        renderAppointments(medicalData?.appointments || []);
        renderTestResults(medicalData?.testResults || []);
        renderARVRegimens(medicalData?.arvRegimens || [], medications);
        renderPayments(medicalData?.payments || []);
        const btnContainer = document.getElementById('createTestResultContainer');
        if (btnContainer) {
            btnContainer.style.display = window.isStaff && window.pmrId != null ? '' : 'none';
        }
    } catch (error) {
        console.error('Error loading patient data:', error);
        document.body.innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Error loading patient data. Please try again.</p>
            </div>
        `;
    }
}

// Utility to get patient ID from URL (assumed)
function getPatientIdFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('patientId');
}