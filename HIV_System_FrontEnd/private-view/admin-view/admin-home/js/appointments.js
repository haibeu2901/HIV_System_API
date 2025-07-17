// Appointments Module
class AppointmentManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load appointments
    async loadAppointments() {
        const appointmentsList = document.getElementById('appointments-list');
        appointmentsList.innerHTML = '<div class="loader"></div>';
        
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
                console.log('Appointments data:', appointments);
                
                if (!appointments || appointments.length === 0) {
                    appointmentsList.innerHTML = '<div class="no-data">Không tìm thấy lịch hẹn nào</div>';
                    return;
                }
                
                this.renderAppointments(appointments);
                this.addCreateAppointmentButton();
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading appointments:', error);
            appointmentsList.innerHTML = '<div class="error-message">Lỗi tải danh sách lịch hẹn. Vui lòng thử lại.</div>';
        }
    }

    // Render appointments
    renderAppointments(appointments) {
        const appointmentsList = document.getElementById('appointments-list');
        appointmentsList.innerHTML = '';
        
        appointments.forEach(appointment => {
            const listItem = this.createAppointmentItem(appointment);
            appointmentsList.appendChild(listItem);
        });
    }

    // Create appointment item
    createAppointmentItem(appointment) {
        const listItem = document.createElement('div');
        listItem.className = 'list-item';
        
        const itemContent = document.createElement('div');
        itemContent.className = 'item-content';
        
        const itemTitle = document.createElement('div');
        itemTitle.className = 'item-title';
        itemTitle.textContent = `Lịch hẹn #${appointment.appointmentId}`;
        
        const itemSubtitle = document.createElement('div');
        itemSubtitle.className = 'item-subtitle';
        itemSubtitle.textContent = `Bệnh nhân: ${appointment.patientName} | Bác sĩ: ${appointment.doctorName}`;
        
        const appointmentDetails = document.createElement('div');
        appointmentDetails.className = 'appointment-details';
        appointmentDetails.innerHTML = `
            <small><strong>Ngày:</strong> ${this.formatDate(appointment.apmtDate)}</small><br>
            <small><strong>Thời gian:</strong> ${appointment.apmTime}</small><br>
            <small><strong>Ghi chú:</strong> ${appointment.notes || 'Không có ghi chú'}</small><br>
            <small><strong>Mã BN:</strong> ${appointment.patientId} | <strong>Mã BS:</strong> ${appointment.doctorId}</small>
        `;
        
        itemContent.appendChild(itemTitle);
        itemContent.appendChild(itemSubtitle);
        itemContent.appendChild(appointmentDetails);
        
        const itemActions = document.createElement('div');
        itemActions.className = 'item-actions';
        
        // Status badge
        const statusBadge = document.createElement('span');
        statusBadge.className = `status-badge status-${this.getStatusClassFromNumber(appointment.apmStatus)}`;
        statusBadge.textContent = this.getStatusTextFromNumber(appointment.apmStatus);
        itemActions.appendChild(statusBadge);
        
        // Action buttons
        const viewBtn = this.createAppointmentButton(appointment.appointmentId, 'view', 'Xem', 'btn-info');
        const editBtn = this.createAppointmentButton(appointment.appointmentId, 'edit', 'Sửa', 'btn-edit');
        const deleteBtn = this.createAppointmentButton(appointment.appointmentId, 'delete', 'Xóa', 'btn-delete');
        
        itemActions.appendChild(viewBtn);
        itemActions.appendChild(editBtn);
        itemActions.appendChild(deleteBtn);
        
        listItem.appendChild(itemContent);
        listItem.appendChild(itemActions);
        
        return listItem;
    }

    // Create appointment button
    createAppointmentButton(appointmentId, action, text, className) {
        const buttonTexts = {
            'view': 'Xem',
            'edit': 'Sửa',
            'delete': 'Xóa'
        };
        
        const button = document.createElement('button');
        button.className = `btn-small ${className}`;
        button.textContent = text;
        
        switch(action) {
            case 'view':
                button.addEventListener('click', () => this.viewAppointment(appointmentId));
                break;
            case 'edit':
                button.addEventListener('click', () => this.editAppointment(appointmentId));
                break;
            case 'delete':
                button.addEventListener('click', () => this.deleteAppointmentConfirm(appointmentId));
                break;
        }
        
        return button;
    }

    // View appointment
    async viewAppointment(appointmentId) {
        try {
            const appointment = await this.getAppointmentById(appointmentId);
            this.showAppointmentModal(appointment);
        } catch (error) {
            console.error('Error viewing appointment:', error);
            alert('Lỗi khi tải thông tin chi tiết cuộc hẹn');
        }
    }

    // Edit appointment
    async editAppointment(appointmentId) {
        try {
            const appointment = await this.getAppointmentById(appointmentId);
            this.showEditAppointmentModal(appointment);
        } catch (error) {
            console.error('Error editing appointment:', error);
            alert('Lỗi khi tải cuộc hẹn để chỉnh sửa');
        }
    }

    // Delete appointment confirmation
    async deleteAppointmentConfirm(appointmentId) {
        if (confirm('Bạn có chắc chắn muốn xóa lịch hẹn này? Hành động này không thể hoàn tác.')) {
            try {
                await this.deleteAppointment(appointmentId);
                alert('Đã xóa lịch hẹn thành công!');
                this.loadAppointments();
            } catch (error) {
                console.error('Error deleting appointment:', error);
                alert('Lỗi khi xóa lịch hẹn. Vui lòng thử lại.');
            }
        }
    }

    // API Methods
    async getAppointmentById(id) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch(`https://localhost:7009/api/Appointment/GetAppointmentById/${id}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                return await response.json();
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error fetching appointment:', error);
            throw error;
        }
    }

    async createAppointment(appointmentData) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Appointment/CreateAppointment', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(appointmentData)
            });
            
            if (response.ok) {
                return await response.json();
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error creating appointment:', error);
            throw error;
        }
    }

    async updateAppointment(id, appointmentData) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch(`https://localhost:7009/api/Appointment/UpdateAppointment/${id}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(appointmentData)
            });
            
            if (response.ok) {
                return await response.json();
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error updating appointment:', error);
            throw error;
        }
    }

    async deleteAppointment(id) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch(`https://localhost:7009/api/Appointment/DeleteAppointment/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                return true;
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error deleting appointment:', error);
            throw error;
        }
    }

    // Modal methods
    showAppointmentModal(appointment) {
        // Create modal if it doesn't exist
        let modal = document.getElementById('appointmentModal');
        if (!modal) {
            modal = document.createElement('div');
            modal.id = 'appointmentModal';
            modal.className = 'modal';
            modal.innerHTML = `
                <div class="modal-content">
                    <div class="modal-header">
                        <h3>Chi tiết lịch hẹn</h3>
                        <button class="close-btn" onclick="window.modalManager.closeModal('appointmentModal')">&times;</button>
                    </div>
                    <div class="modal-body" id="appointmentContent">
                        <!-- Content will be populated here -->
                    </div>
                </div>
            `;
            document.body.appendChild(modal);
        }
        
        // Populate modal content
        document.getElementById('appointmentContent').innerHTML = `
            <div class="appointment-details">
                <div class="detail-section">
                    <h4><i class="fas fa-calendar-alt"></i> Thông tin lịch hẹn</h4>
                    <p><strong>Mã lịch hẹn:</strong> ${appointment.appointmentId}</p>
                    <p><strong>Ngày:</strong> ${this.formatDate(appointment.apmtDate)}</p>
                    <p><strong>Thời gian:</strong> ${appointment.apmTime}</p>
                    <p><strong>Trạng thái:</strong> <span class="status-badge status-${this.getStatusClassFromNumber(appointment.apmStatus)}">${this.getStatusTextFromNumber(appointment.apmStatus)}</span></p>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-user"></i> Thông tin bệnh nhân</h4>
                    <p><strong>Họ tên:</strong> ${appointment.patientName}</p>
                    <p><strong>Mã BN:</strong> ${appointment.patientId}</p>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-user-md"></i> Thông tin bác sĩ</h4>
                    <p><strong>Họ tên:</strong> ${appointment.doctorName}</p>
                    <p><strong>Mã BS:</strong> ${appointment.doctorId}</p>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-file-medical-alt"></i> Ghi chú</h4>
                    <div class="notes-content">
                        ${appointment.notes || 'Không có ghi chú cho lịch hẹn này.'}
                    </div>
                </div>
            </div>
        `;
        
        // Show modal
        modal.style.display = 'block';
    }

    showEditAppointmentModal(appointment) {
        // Create modal if it doesn't exist
        let modal = document.getElementById('editAppointmentModal');
        if (!modal) {
            modal = document.createElement('div');
            modal.id = 'editAppointmentModal';
            modal.className = 'modal';
            modal.innerHTML = `
                <div class="modal-content">
                    <div class="modal-header">
                        <h3>Chỉnh sửa cuộc hẹn</h3>
                        <button class="close-btn" onclick="window.modalManager.closeModal('editAppointmentModal')">&times;</button>
                    </div>
                    <div class="modal-body">
                        <form id="editAppointmentForm">
                            <input type="hidden" id="edit-appointment-id" value="${appointment.appointmentId}">
                            
                            <div class="form-group">
                                <label for="edit-patient-id">ID bệnh nhân</label>
                                <input type="number" id="edit-patient-id" value="${appointment.patientId}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-doctor-id">ID bác sĩ</label>
                                <input type="number" id="edit-doctor-id" value="${appointment.doctorId}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-appointment-date">Ngày</label>
                                <input type="date" id="edit-appointment-date" value="${appointment.apmtDate}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-appointment-time">Thời gian</label>
                                <input type="time" id="edit-appointment-time" value="${appointment.apmTime}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-appointment-status">Trạng thái</label>
                                <select id="edit-appointment-status" required>
                                    <option value="1"${appointment.apmStatus === 1 ? ' selected' : ''}>Chưa giải quyết</option>
                                    <option value="2"${appointment.apmStatus === 2 ? ' selected' : ''}>Đã xác nhận</option>
                                    <option value="3"${appointment.apmStatus === 3 ? ' selected' : ''}>Hoàn thành</option>
                                    <option value="4"${appointment.apmStatus === 4 ? ' selected' : ''}>Đã hủy</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-appointment-notes">Ghi chú</label>
                                <textarea id="edit-appointment-notes" rows="4">${appointment.notes || ''}</textarea>
                            </div>
                            
                            <div class="form-actions">
                                <button type="button" onclick="window.modalManager.closeModal('editAppointmentModal')">Hủy bỏ</button>
                                <button type="submit">Cập nhật cuộc hẹn</button>
                            </div>
                        </form>
                    </div>
                </div>
            `;
            document.body.appendChild(modal);
        }
        
        // Show modal
        modal.style.display = 'block';
    }

    showCreateAppointmentModal() {
        // Create modal if it doesn't exist
        let modal = document.getElementById('createAppointmentModal');
        if (!modal) {
            modal = document.createElement('div');
            modal.id = 'createAppointmentModal';
            modal.className = 'modal';
            modal.innerHTML = `
                <div class="modal-content">
                    <div class="modal-header">
                        <h3>Tạo lịch hẹn mới</h3>
                        <button class="close-btn" onclick="window.modalManager.closeModal('createAppointmentModal')">&times;</button>
                    </div>
                    <div class="modal-body">
                        <form id="createAppointmentForm">
                            <div class="form-group">
                                <label for="patient-id">ID bệnh nhân</label>
                                <input type="number" id="patient-id" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="doctor-id">ID bác sĩ</label>
                                <input type="number" id="doctor-id" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="appointment-date">Ngày</label>
                                <input type="date" id="appointment-date" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="appointment-time">Thời gian</label>
                                <input type="time" id="appointment-time" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="appointment-status">Trạng thái</label>
                                <select id="appointment-status" required>
                                    <option value="1">Chờ xác nhận</option>
                                    <option value="2">Đã xác nhận</option>
                                    <option value="3">Hoàn thành</option>
                                    <option value="4">Đã hủy</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="appointment-notes">Ghi chú</label>
                                <textarea id="appointment-notes" rows="4"></textarea>
                            </div>
                            
                            <div class="form-actions">
                                <button type="button" onclick="window.modalManager.closeModal('createAppointmentModal')">Huỷ bỏ</button>
                                <button type="submit">Tạo cuộc hẹn</button>
                            </div>
                        </form>
                    </div>
                </div>
            `;
            document.body.appendChild(modal);
        }
        
        // Show modal
        modal.style.display = 'block';
    }

    // Add create appointment button
    addCreateAppointmentButton() {
        const sectionHeader = document.querySelector('#appointments .section-header');
        if (sectionHeader && !sectionHeader.querySelector('.create-appointment-btn')) {
            const createButton = document.createElement('button');
            createButton.className = 'btn-primary create-appointment-btn';
            createButton.innerHTML = '<i class="fas fa-plus"></i> Tạo lịch hẹn mới';
            createButton.onclick = () => this.showCreateAppointmentModal();
            sectionHeader.appendChild(createButton);
        }
    }

    // Filter appointments
    filterAppointments() {
        const dateFilter = document.getElementById('appointment-date-filter').value;
        const statusFilter = document.getElementById('appointment-status-filter').value;
        
        const appointmentsList = document.getElementById('appointments-list');
        appointmentsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        fetch('https://localhost:7009/api/Appointment/GetAllAppointments', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(appointments => {
            let filteredAppointments = appointments;
            
            // Filter by date
            if (dateFilter) {
                filteredAppointments = filteredAppointments.filter(appointment => 
                    appointment.apmtDate === dateFilter
                );
            }
            
            // Filter by status
            if (statusFilter && statusFilter !== 'all') {
                filteredAppointments = filteredAppointments.filter(appointment => 
                    appointment.apmStatus.toString() === statusFilter
                );
            }
            
            if (filteredAppointments.length === 0) {
                appointmentsList.innerHTML = '<div class="no-data">Không tìm thấy lịch hẹn nào phù hợp với bộ lọc</div>';
                return;
            }
            
            this.renderAppointments(filteredAppointments);
        })
        .catch(error => {
            console.error('Error filtering appointments:', error);
            appointmentsList.innerHTML = '<div class="error-message">Có lỗi khi lọc cuộc hẹn. Vui lòng thử lại.</div>';
        });
    }

    // Initialize appointment form handlers
    initFormHandlers() {
        // Create Appointment Form Handler
        document.addEventListener('submit', (e) => {
            if (e.target.id === 'createAppointmentForm') {
                e.preventDefault();
                
                const formData = {
                    patientId: parseInt(document.getElementById('patient-id').value),
                    doctorId: parseInt(document.getElementById('doctor-id').value),
                    apmtDate: document.getElementById('appointment-date').value,
                    apmTime: document.getElementById('appointment-time').value,
                    apmStatus: parseInt(document.getElementById('appointment-status').value),
                    notes: document.getElementById('appointment-notes').value
                };
                
                const submitButton = e.target.querySelector('button[type="submit"]');
                window.utilsManager.showButtonLoader(submitButton, 'Creating...');
                
                this.createAppointment(formData)
                    .then(() => {
                        alert('Đã tạo cuộc hẹn thành công!');
                        window.modalManager.closeModal('createAppointmentModal');
                        this.loadAppointments();
                    })
                    .catch(error => {
                        console.error('Error creating appointment:', error);
                        alert('Có lỗi khi tạo cuộc hẹn. Vui lòng thử lại.');
                    })
                    .finally(() => {
                        window.utilsManager.hideButtonLoader(submitButton);
                    });
            }
        });
        
        // Edit Appointment Form Handler
        document.addEventListener('submit', (e) => {
            if (e.target.id === 'editAppointmentForm') {
                e.preventDefault();
                
                const appointmentId = parseInt(document.getElementById('edit-appointment-id').value);
                const formData = {
                    patientId: parseInt(document.getElementById('edit-patient-id').value),
                    doctorId: parseInt(document.getElementById('edit-doctor-id').value),
                    apmtDate: document.getElementById('edit-appointment-date').value,
                    apmTime: document.getElementById('edit-appointment-time').value,
                    apmStatus: parseInt(document.getElementById('edit-appointment-status').value),
                    notes: document.getElementById('edit-appointment-notes').value
                };
                
                const submitButton = e.target.querySelector('button[type="submit"]');
                window.utilsManager.showButtonLoader(submitButton, 'Updating...');
                
                this.updateAppointment(appointmentId, formData)
                    .then(() => {
                        alert('Đã cập nhật cuộc hẹn thành công!');
                        window.modalManager.closeModal('editAppointmentModal');
                        this.loadAppointments();
                    })
                    .catch(error => {
                        console.error('Error updating appointment:', error);
                        alert('Có lỗi khi cập nhật lịch hẹn. Vui lòng thử lại.');
                    })
                    .finally(() => {
                        window.utilsManager.hideButtonLoader(submitButton);
                    });
            }
        });
    }

    // Helper methods
    formatDate(dateString) {
        if (!dateString) return 'N/A';
        
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch (error) {
            return dateString;
        }
    }

    getStatusClassFromNumber(statusNumber) {
        switch (statusNumber) {
            case 1: return 'pending';
            case 2: return 'confirmed';
            case 3: return 'completed';
            case 4: return 'cancelled';
            default: return 'pending';
        }
    }

    getStatusTextFromNumber(statusNumber) {
        switch (statusNumber) {
            case 1: return 'Chờ xác nhận';
            case 2: return 'Đã xác nhận';
            case 3: return 'Đã hoàn thành';
            case 4: return 'Đã hủy';
            default: return 'Không xác định';
        }
    }

    // Initialize
    init() {
        this.initFormHandlers();
        
        // Filter elements
        const appointmentDateFilter = document.getElementById('appointment-date-filter');
        if (appointmentDateFilter) {
            appointmentDateFilter.addEventListener('change', () => this.filterAppointments());
        }
        
        const appointmentStatusFilter = document.getElementById('appointment-status-filter');
        if (appointmentStatusFilter) {
            appointmentStatusFilter.addEventListener('change', () => this.filterAppointments());
        }
    }
}

// Export for use in other modules
window.AppointmentManager = AppointmentManager;
