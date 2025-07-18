// Accounts Module
class AccountManager {
    constructor(authManager) {
        this.authManager = authManager;
    }

    // Load accounts
    async loadAccounts() {
        const accountsList = document.getElementById('accounts-list');
        accountsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Account/GetAllAccounts', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                console.log('Accounts data:', data);
                
                if (!data || data.length === 0) {
                    accountsList.innerHTML = '<div class="no-data">Không tìm thấy tài khoản nào</div>';
                    return;
                }
                
                this.renderAccounts(data);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading accounts:', error);
            accountsList.innerHTML = '<div class="error-message">Lỗi tải danh sách tài khoản. Vui lòng thử lại.</div>';
        }
    }

    // Show create account modal
    showCreateAccountModal() {
        const modal = document.getElementById('createAccountModal');
        if (modal) {
            modal.style.display = 'flex';
        }
    }

    // Filter accounts
    async filterAccounts() {
        const roleFilter = document.getElementById('account-role-filter').value;
        const searchTerm = document.getElementById('account-search').value.toLowerCase();
        
        console.log('Filtering accounts by:', roleFilter, searchTerm);
        
        const accountsList = document.getElementById('accounts-list');
        accountsList.innerHTML = '<div class="loader"></div>';
        
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch('https://localhost:7009/api/Account/GetAllAccounts', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                let data = await response.json();
                
                // Apply filters
                if (roleFilter !== 'all') {
                    data = data.filter(account => {
                        const userRole = this.detectUserRole(account.accUsername);
                        const roleMapping = {
                            '1': 'admin',
                            '2': 'doctor', 
                            '3': 'patient',
                            '4': 'staff',
                            '5': 'supervisor'
                        };
                        return roleMapping[roleFilter] === userRole;
                    });
                }
                
                if (searchTerm) {
                    data = data.filter(account => 
                        account.fullname?.toLowerCase().includes(searchTerm) ||
                        account.accUsername?.toLowerCase().includes(searchTerm) ||
                        account.email?.toLowerCase().includes(searchTerm)
                    );
                }
                
                if (data.length === 0) {
                    accountsList.innerHTML = '<div class="no-data">Không tìm thấy tài khoản nào phù hợp với tiêu chí</div>';
                    return;
                }
                
                this.renderAccounts(data);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error filtering accounts:', error);
            accountsList.innerHTML = '<div class="error-message">Có lỗi khi lọc tài khoản. Vui lòng thử lại.</div>';
        }
    }

    // Edit account
    async editAccount(id) {
        console.log('Edit account:', id);
        
        const token = this.authManager.getToken();
        
        try {
            // Get account details first
            const accountResponse = await fetch('https://localhost:7009/api/Account/GetAllAccounts', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (accountResponse.ok) {
                const accounts = await accountResponse.json();
                const account = accounts.find(acc => acc.accId === id);
                
                if (account) {
                    this.showEditAccountModal(account);
                } else {
                    throw new Error('Account not found');
                }
            } else {
                throw new Error(`HTTP error! status: ${accountResponse.status}`);
            }
        } catch (error) {
            console.error('Error loading account for edit:', error);
            window.utils.showToast('Có lỗi khi tải thông tin tài khoản. Vui lòng thử lại.', 'error');
        }
    }

    // Delete account
    async deleteAccount(id) {
        if (confirm('Bạn có chắc chắn muốn xóa tài khoản này? Hành động này không thể hoàn tác.')) {
            const token = this.authManager.getToken();
            
            try {
                const response = await fetch(`https://localhost:7009/api/Account/DeleteAccount/${id}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'accept': '*/*'
                    }
                });
                
                if (response.ok) {
                    window.utils.showToast('Xóa tài khoản thành công!', 'success');
                    this.loadAccounts(); // Reload the accounts list
                } else {
                    const errorData = await response.text();
                    throw new Error(errorData || `HTTP error! status: ${response.status}`);
                }
            } catch (error) {
                console.error('Error deleting account:', error);
                window.utils.showToast('Có lỗi khi xóa tài khoản. Vui lòng thử lại.', 'error');
            }
        }
    }

    // Handle create account form
    handleCreateAccount(e) {
        e.preventDefault();
        
        const username = document.getElementById('account-username').value;
        const email = document.getElementById('account-email').value;
        const fullname = document.getElementById('account-fullname').value;
        const role = document.getElementById('account-role').value;
        const password = document.getElementById('account-password').value;
        
        console.log('Creating account:', { username, email, fullname, role, password });
        
        alert('Account created successfully!');
        window.modalManager.closeModal('createAccountModal');
        this.loadAccounts();
    }

    // Render accounts
    renderAccounts(accounts) {
        const accountsList = document.getElementById('accounts-list');
        
        // Sort accounts by role and then by username
        const sortedAccounts = accounts.sort((a, b) => {
            const roleA = this.detectUserRole(a.accUsername);
            const roleB = this.detectUserRole(b.accUsername);
            
            if (roleA !== roleB) {
                return this.getRolePriority(roleA) - this.getRolePriority(roleB);
            }
            
            return a.accUsername.localeCompare(b.accUsername);
        });
        
        const accountsHTML = sortedAccounts.map(account => {
            const userRole = this.detectUserRole(account.accUsername);
            const roleName = this.getRoleDisplayName(userRole);
            const roleClass = this.getRoleClass(userRole);
            
            return `
                <div class="account-card">
                    <div class="account-header">
                        <div class="account-avatar">
                            <i class="fas ${this.getRoleIcon(userRole)}"></i>
                        </div>
                        <div class="account-info">
                            <h3>${account.fullname || 'N/A'}</h3>
                            <p class="account-username">@${account.accUsername}</p>
                            <p class="account-email">${account.email}</p>
                        </div>
                        <div class="account-role">
                            <span class="role-badge ${roleClass}">${roleName}</span>
                        </div>
                    </div>
                    
                    <div class="account-details">
                        <div class="detail-item">
                            <i class="fas fa-id-card"></i>
                            <span>ID: ${account.accId}</span>
                        </div>
                        <div class="detail-item">
                            <i class="fas fa-user-tag"></i>
                            <span>Tên đăng nhập: ${account.accUsername}</span>
                        </div>
                        <div class="detail-item">
                            <i class="fas fa-shield-alt"></i>
                            <span>Vai trò: ${roleName}</span>
                        </div>
                    </div>
                    
                    <div class="account-actions">
                        <button class="btn-secondary" onclick="accountManager.viewAccount(${account.accId})">
                            <i class="fas fa-eye"></i> Xem
                        </button>
                        <button class="btn-info" onclick="accountManager.showSendNotificationModal(${account.accId}, '${account.accUsername}', '${account.fullname || 'N/A'}')">
                            <i class="fas fa-bell"></i> Gửi thông báo
                        </button>
                        ${userRole !== 'admin' ? `
                            <button class="btn-primary" onclick="accountManager.editAccount(${account.accId})">
                                <i class="fas fa-edit"></i> Sửa
                            </button>
                            <button class="btn-danger" onclick="accountManager.deleteAccount(${account.accId})">
                                <i class="fas fa-trash"></i> Xóa
                            </button>
                        ` : ''}
                    </div>
                </div>
            `;
        }).join('');
        
        accountsList.innerHTML = accountsHTML;
    }

    // Detect user role based on username
    detectUserRole(username) {
        const lowerUsername = username.toLowerCase();
        
        if (lowerUsername.includes('admin')) return 'admin';
        if (lowerUsername.includes('doctor')) return 'doctor';
        if (lowerUsername.includes('patient')) return 'patient';
        if (lowerUsername.includes('staff')) return 'staff';
        if (lowerUsername.includes('supervisor')) return 'supervisor';
        
        // Default to patient for unknown roles
        return 'patient';
    }

    // Get role priority for sorting
    getRolePriority(role) {
        const priorities = {
            'admin': 1,
            'supervisor': 2,
            'doctor': 3,
            'staff': 4,
            'patient': 5
        };
        return priorities[role] || 999;
    }

    // Get role display name
    getRoleDisplayName(role) {
        const roleNames = {
            'admin': 'Quản trị viên',
            'supervisor': 'Giám sát viên',
            'doctor': 'Bác sĩ',
            'staff': 'Nhân viên',
            'patient': 'Bệnh nhân'
        };
        return roleNames[role] || 'Không xác định';
    }

    // Get role CSS class
    getRoleClass(role) {
        const roleClasses = {
            'admin': 'role-admin',
            'supervisor': 'role-supervisor',
            'doctor': 'role-doctor',
            'staff': 'role-staff',
            'patient': 'role-patient'
        };
        return roleClasses[role] || 'role-unknown';
    }

    // Get role icon
    getRoleIcon(role) {
        const roleIcons = {
            'admin': 'fa-user-shield',
            'supervisor': 'fa-user-tie',
            'doctor': 'fa-user-md',
            'staff': 'fa-user-cog',
            'patient': 'fa-user'
        };
        return roleIcons[role] || 'fa-user-circle';
    }

    // View account details
    async viewAccount(accId) {
        console.log('Viewing account:', accId);
        
        const token = this.authManager.getToken();
        
        try {
            // First get the account details to determine user type
            const accountResponse = await fetch('https://localhost:7009/api/Account/GetAllAccounts', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (accountResponse.ok) {
                const accounts = await accountResponse.json();
                const account = accounts.find(acc => acc.accId === accId);
                
                if (account) {
                    const userRole = this.detectUserRole(account.accUsername);
                    
                    if (userRole === 'patient') {
                        this.viewPatientProfile(accId);
                    } else if (userRole === 'doctor') {
                        this.viewDoctorProfile(accId);
                    } else {
                        this.viewGeneralProfile(account);
                    }
                } else {
                    throw new Error('Account not found');
                }
            } else {
                throw new Error(`HTTP error! status: ${accountResponse.status}`);
            }
        } catch (error) {
            console.error('Error viewing account:', error);
            window.utils.showToast('Có lỗi khi tải thông tin tài khoản. Vui lòng thử lại.', 'error');
        }
    }

    // View patient profile
    async viewPatientProfile(accId) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch(`https://localhost:7009/api/Account/GetPatientProfile/${accId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const patientData = await response.json();
                this.showPatientProfileModal(patientData);
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading patient profile:', error);
            window.utils.showToast('Có lỗi khi tải hồ sơ bệnh nhân. Vui lòng thử lại.', 'error');
        }
    }

    // View doctor profile
    async viewDoctorProfile(accId) {
        const token = this.authManager.getToken();
        
        try {
            const response = await fetch(`https://localhost:7009/api/Account/GetDoctorProfile/${accId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (response.ok) {
                const doctorData = await response.json();
                this.showDoctorProfileModal(doctorData);
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading doctor profile:', error);
            window.utils.showToast('Có lỗi khi tải hồ sơ bác sĩ. Vui lòng thử lại.', 'error');
        }
    }

    // View general profile (for admin, staff, supervisor)
    viewGeneralProfile(account) {
        this.showGeneralProfileModal(account);
    }

    // Show patient profile modal
    showPatientProfileModal(patientData) {
        const modalHTML = `
            <div id="patientProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 800px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-user"></i> Patient Profile</h3>
                        <button class="close-btn" onclick="accountManager.closeProfileModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="profile-container">
                            <div class="profile-header">
                                <div class="profile-avatar">
                                    <i class="fas fa-user-circle"></i>
                                </div>
                                <div class="profile-info">
                                    <h2>${patientData.fullname || 'N/A'}</h2>
                                    <p class="profile-username">@${patientData.accUsername}</p>
                                    <span class="role-badge role-patient">Bệnh nhân</span>
                                </div>
                            </div>
                            
                            <div class="profile-details">
                                <div class="detail-section">
                                    <h4><i class="fas fa-info-circle"></i>Thông tin tài khoản</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Account ID:</label>
                                            <span>${patientData.accId}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Email:</label>
                                            <span>${patientData.email}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Tên người dùng:</label>
                                            <span>${patientData.accUsername}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-user-md"></i>Chi tiết bệnh nhân</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>ID bệnh nhân:</label>
                                            <span>${patientData.ptnId || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Điện thoại:</label>
                                            <span>${patientData.ptnPhone || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Địa chỉ:</label>
                                            <span>${patientData.ptnAddress || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Ngày sinh:</label>
                                            <span>${patientData.ptnDateOfBirth ? new Date(patientData.ptnDateOfBirth).toLocaleDateString() : 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Giới tính:</label>
                                            <span>${patientData.ptnGender || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Liên hệ khẩn cấp:</label>
                                            <span>${patientData.ptnEmergencyContact || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-notes-medical"></i>Thông tin y tế</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Số bảo hiểm:</label>
                                            <span>${patientData.ptnInsuranceNumber || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Ghi chú y tế:</label>
                                            <span>${patientData.ptnMedicalNotes || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Đóng</button>
                        <button class="btn-primary" onclick="accountManager.editPatientProfile(${patientData.accId})">
                            <i class="fas fa-edit"></i> Chỉnh sửa hồ sơ
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }

    // Show doctor profile modal
    showDoctorProfileModal(doctorData) {
        const modalHTML = `
            <div id="doctorProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 800px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-user-md"></i> Hồ sơ bác sĩ</h3>
                        <button class="close-btn" onclick="accountManager.closeProfileModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="profile-container">
                            <div class="profile-header">
                                <div class="profile-avatar">
                                    <i class="fas fa-user-md"></i>
                                </div>
                                <div class="profile-info">
                                    <h2>${doctorData.fullname || 'N/A'}</h2>
                                    <p class="profile-username">@${doctorData.accUsername}</p>
                                    <span class="role-badge role-doctor">Bác sĩ</span>
                                </div>
                            </div>
                            
                            <div class="profile-details">
                                <div class="detail-section">
                                    <h4><i class="fas fa-info-circle"></i> Thông tin tài khoản</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>ID tài khoản:</label>
                                            <span>${doctorData.accId}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Email:</label>
                                            <span>${doctorData.email}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Tên người dùng:</label>
                                            <span>${doctorData.accUsername}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-stethoscope"></i> Chi tiết bác sĩ</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>ID bác sĩ:</label>
                                            <span>${doctorData.docId || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Phone:</label>
                                            <span>${doctorData.docPhone || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Địa chỉ:</label>
                                            <span>${doctorData.docAddress || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Ngày sinh:</label>
                                            <span>${doctorData.docDateOfBirth ? new Date(doctorData.docDateOfBirth).toLocaleDateString() : 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Giới tính:</label>
                                            <span>${doctorData.docGender || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Chuyên môn:</label>
                                            <span>${doctorData.docSpecialization || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-certificate"></i> Thông tin chuyên môn</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Số giấy phép:</label>
                                            <span>${doctorData.docLicenseNumber || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Số năm kinh nghiệm:</label>
                                            <span>${doctorData.docYearsOfExperience || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Trình độ chuyên môn:</label>
                                            <span>${doctorData.docQualification || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Đóng</button>
                        <button class="btn-primary" onclick="accountManager.editDoctorProfile(${doctorData.accId})">
                            <i class="fas fa-edit"></i> Chỉnh sửa hồ sơ
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }

    // Show general profile modal
    showGeneralProfileModal(account) {
        const userRole = this.detectUserRole(account.accUsername);
        const roleName = this.getRoleDisplayName(userRole);
        const roleClass = this.getRoleClass(userRole);
        
        const modalHTML = `
            <div id="generalProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 600px;">
                    <div class="modal-header">
                        <h3><i class="fas ${this.getRoleIcon(userRole)}"></i> ${roleName} Hồ sơ</h3>
                        <button class="close-btn" onclick="accountManager.closeProfileModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="profile-container">
                            <div class="profile-header">
                                <div class="profile-avatar">
                                    <i class="fas ${this.getRoleIcon(userRole)}"></i>
                                </div>
                                <div class="profile-info">
                                    <h2>${account.fullname || 'N/A'}</h2>
                                    <p class="profile-username">@${account.accUsername}</p>
                                    <span class="role-badge ${roleClass}">${roleName}</span>
                                </div>
                            </div>
                            
                            <div class="profile-details">
                                <div class="detail-section">
                                    <h4><i class="fas fa-info-circle"></i> Thông tin tài khoản</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>ID tài khoản:</label>
                                            <span>${account.accId}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Email:</label>
                                            <span>${account.email}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Tên người dùng:</label>
                                            <span>${account.accUsername}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Họ và tên đầy đủ:</label>
                                            <span>${account.fullname}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Vai trò:</label>
                                            <span>${roleName}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Đóng</button>
                        <button class="btn-primary" onclick="accountManager.editGeneralProfile(${account.accId})">
                            <i class="fas fa-edit"></i> Chỉnh sửa hồ sơ
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }

    // Close profile modal
    closeProfileModal() {
        const modals = ['patientProfileModal', 'doctorProfileModal', 'generalProfileModal'];
        modals.forEach(modalId => {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.remove();
            }
        });
    }

    // Edit patient profile
    editPatientProfile(accId) {
        console.log('Edit patient profile:', accId);
        // Implementation for editing patient profile
        window.utils.showToast('Chức năng chỉnh sửa hồ sơ bệnh nhân sẽ sớm ra mắt!', 'info');
    }

    // Edit doctor profile
    editDoctorProfile(accId) {
        console.log('Edit doctor profile:', accId);
        // Implementation for editing doctor profile
        window.utils.showToast('Chức năng chỉnh sửa hồ sơ bác sĩ sẽ sớm ra mắt!', 'info');
    }

    // Edit general profile
    editGeneralProfile(accId) {
        console.log('Edit general profile:', accId);
        // Implementation for editing general profile
        window.utils.showToast('Chức năng chỉnh sửa hồ sơ sẽ sớm ra mắt!', 'info');
    }

    // Show edit account modal
    showEditAccountModal(account) {
        const userRole = this.detectUserRole(account.accUsername);
        const roleName = this.getRoleDisplayName(userRole);
        
        const modalHTML = `
            <div id="editAccountModal" class="modal" style="display: none;">
                <div class="modal-content" style="max-width: 600px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-edit"></i> Chỉnh sửa tài khoản</h3>
                        <button class="close-btn" onclick="accountManager.closeEditModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <form id="editAccountForm">
                            <input type="hidden" id="edit-account-id" value="${account.accId}">
                            
                            <div class="form-group">
                                <label for="edit-account-password">Mật khẩu mới</label>
                                <input type="password" id="edit-account-password" placeholder="Nhập mật khẩu mới (không bắt buộc)" value="">
                                <small class="form-text">Để trống nếu không muốn thay đổi mật khẩu</small>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-email">Email</label>
                                <input type="email" id="edit-account-email" value="${account.email}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-fullname">Họ tên</label>
                                <input type="text" id="edit-account-fullname" value="${account.fullname}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-dob">Ngày sinh</label>
                                <input type="date" id="edit-account-dob" value="${account.dob ? account.dob.split('T')[0] : ''}">
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-gender">Giới tính</label>
                                <select id="edit-account-gender">
                                    <option value="">Select Gender</option>
                                    <option value="true" ${account.gender === true ? 'selected' : ''}>Nam</option>
                                    <option value="false" ${account.gender === false ? 'selected' : ''}>Nữ</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-roles">Vai trò</label>
                                <select id="edit-account-roles">
                                    <option value="0" ${userRole === 'admin' ? 'selected' : ''}>Quản trị viên</option>
                                    <option value="1" ${userRole === 'doctor' ? 'selected' : ''}>Bác sĩ</option>
                                    <option value="2" ${userRole === 'patient' ? 'selected' : ''}>Bệnh nhân</option>
                                    <option value="3" ${userRole === 'staff' ? 'selected' : ''}>Nhân viên</option>
                                    <option value="4" ${userRole === 'supervisor' ? 'selected' : ''}>Giám sát viên</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-active">Trạng thái tài khoản</label>
                                <select id="edit-account-active">
                                    <option value="true" ${account.isActive !== false ? 'selected' : ''}>Đang hoạt động</option>
                                    <option value="false" ${account.isActive === false ? 'selected' : ''}>Không hoạt động</option>
                                </select>
                            </div>
                            
                            <div class="form-actions">
                                <button type="button" onclick="accountManager.closeEditModal()">Cancel</button>
                                <button type="submit">
                                    <i class="fas fa-save"></i> Update Account
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        
        // Show the modal with proper centering
        const modal = document.getElementById('editAccountModal');
        modal.style.display = 'flex';
        
        // Add form submit event listener
        document.getElementById('editAccountForm').addEventListener('submit', (e) => this.handleUpdateAccount(e));
    }

    // Handle update account form submission
    async handleUpdateAccount(e) {
        e.preventDefault();
        
        const accountId = document.getElementById('edit-account-id').value;
        const password = document.getElementById('edit-account-password').value;
        const email = document.getElementById('edit-account-email').value;
        const fullname = document.getElementById('edit-account-fullname').value;
        const dob = document.getElementById('edit-account-dob').value;
        const gender = document.getElementById('edit-account-gender').value;
        const roles = document.getElementById('edit-account-roles').value;
        const isActive = document.getElementById('edit-account-active').value === 'true';
        
        // Validate required fields
        if (!email || !fullname) {
            window.utils.showToast('Vui lòng điền vào tất cả các trường bắt buộc (Email và Họ và tên)', 'error');
            return;
        }
        
        // Validate email format
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            window.utils.showToast('Vui lòng nhập địa chỉ email hợp lệ', 'error');
            return;
        }
        
        const token = this.authManager.getToken();
        
        // Prepare the request body according to the API specification
        const requestBody = {
            email: email,
            fullname: fullname,
            dob: dob || "2025-07-14", // Default date if not provided
            gender: gender !== "" ? gender === "true" : true, // Convert to boolean
            roles: parseInt(roles) || 0,
            isActive: isActive
        };
        
        // Only include password if provided
        if (password && password.trim() !== '') {
            requestBody.accPassword = password;
        }
        
        try {
            const submitButton = e.target.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Updating...';
            }
            
            console.log('Updating account with data:', requestBody);
            
            const response = await fetch(`https://localhost:7009/api/Account/UpdateAccount/${accountId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                window.utils.showToast('Tài khoản đã được cập nhật thành công!', 'success');
                this.closeEditModal();
                this.loadAccounts(); // Reload the accounts list
            } else {
                const errorData = await response.text();
                console.error('Update failed:', errorData);
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error updating account:', error);
            window.utils.showToast('Có lỗi khi cập nhật tài khoản. Vui lòng thử lại.', 'error');
        } finally {
            const submitButton = e.target.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-save"></i> Cập nhật tài khoản';
            }
        }
    }

    // Close edit modal
    closeEditModal() {
        const modal = document.getElementById('editAccountModal');
        if (modal) {
            modal.remove();
        }
    }

    // Initialize
    init() {
        // Create account button
        const createAccountBtn = document.getElementById('create-account-btn');
        if (createAccountBtn) {
            createAccountBtn.addEventListener('click', () => this.showCreateAccountModal());
        }
        
        // Filter elements
        const accountRoleFilter = document.getElementById('account-role-filter');
        if (accountRoleFilter) {
            accountRoleFilter.addEventListener('change', () => this.filterAccounts());
        }
        
        // Search functionality
        const accountSearchInput = document.getElementById('account-search');
        if (accountSearchInput) {
            accountSearchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.filterAccounts();
                }
            });
        }
        
        // Form handler
        const createAccountForm = document.getElementById('createAccountForm');
        if (createAccountForm) {
            createAccountForm.addEventListener('submit', (e) => this.handleCreateAccount(e));
        }
    }

    // Show send notification modal
    showSendNotificationModal(accountId, username, fullname) {
        const modalHTML = `
            <div id="sendNotificationModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 600px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-bell"></i>Gửi thông báo</h3>
                        <button class="close-btn" onclick="accountManager.closeSendNotificationModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="recipient-info">
                            <div class="recipient-card">
                                <div class="recipient-avatar">
                                    <i class="fas fa-user"></i>
                                </div>
                                <div class="recipient-details">
                                    <h4>${fullname}</h4>
                                    <p class="recipient-username">@${username}</p>
                                    <p class="recipient-id">ID Tài khoản: ${accountId}</p>
                                </div>
                            </div>
                        </div>
                        
                        <form id="sendNotificationForm">
                            <input type="hidden" id="notification-account-id" value="${accountId}">
                            
                            <div class="form-group">
                                <label for="notification-type">Loại thông báo</label>
                                <select id="notification-type" required>
                                    <option value="">Chọn loại thông báo</option>
                                    <option value="Cảnh báo HT">Cảnh báo HT</option>
                                    <option value="Xác nhận Hẹn">Xác nhận Hẹn</option>
                                    <option value="Cập nhật Hẹn">Cập nhật Hẹn</option>
                                    <option value="Yêu cầu Hẹn">Yêu cầu Hẹn</option>
                                    <option value="Nhắc Hẹn">Nhắc Hẹn</option>
                                    <option value="Tư vấn ARV">Tư vấn ARV</option>
                                    <option value="Kết quả XN">Kết quả XN</option>
                                    <option value="Duyệt Blog">Duyệt Blog</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="notification-message">Tin nhắn</label>
                                <textarea id="notification-message" rows="4" placeholder="Nhập tin nhắn thông báo của bạn vào đây..." required></textarea>
                                <small class="form-text">Tin nhắn này sẽ được gửi đến ${fullname} (@${username})</small>
                            </div>
                            
                            <div class="form-actions">
                                <button type="button" class="btn-secondary" onclick="accountManager.closeSendNotificationModal()">Hủy bỏ</button>
                                <button type="submit" class="btn-primary">
                                    <i class="fas fa-paper-plane"></i> Gửi thông báo
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;
        
        // Remove existing modal if it exists
        const existingModal = document.getElementById('sendNotificationModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        // Add new modal to DOM
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        
        // Add form submit event listener
        document.getElementById('sendNotificationForm').addEventListener('submit', (e) => this.handleSendNotification(e));
    }

    // Close send notification modal
    closeSendNotificationModal() {
        const modal = document.getElementById('sendNotificationModal');
        if (modal) {
            modal.remove();
        }
    }

    // Handle send notification form submission
    async handleSendNotification(e) {
        e.preventDefault();
        
        const accountId = document.getElementById('notification-account-id').value;
        const notiType = document.getElementById('notification-type').value;
        const notiMessage = document.getElementById('notification-message').value;
        
        // Validate input data
        if (!notiType || !notiMessage) {
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Vui lòng điền vào tất cả các trường bắt buộc', 'error');
            } else {
                alert('Vui lòng điền vào tất cả các trường bắt buộc');
            }
            return;
        }
        
        const submitButton = e.target.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';
        }
        
        try {
            const success = await this.sendNotificationToAccount(accountId, notiType, notiMessage);
            
            if (success) {
                if (window.utils && window.utils.showToast) {
                    window.utils.showToast('Thông báo đã được gửi thành công!', 'success');
                } else {
                    alert('Thông báo đã được gửi thành công!');
                }
                
                // Close modal and reset form
                this.closeSendNotificationModal();
            }
        } catch (error) {
            console.error('Error sending notification:', error);
            if (window.utils && window.utils.showToast) {
                window.utils.showToast('Có lỗi khi gửi thông báo. Vui lòng thử lại.', 'error');
            } else {
                alert('Có lỗi khi gửi thông báo. Vui lòng thử lại.');
            }
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi thông báo';
            }
        }
    }

    // Send notification to specific account
    async sendNotificationToAccount(accountId, notiType, notiMessage) {
        const token = this.authManager.getToken();
        
        const requestBody = {
            notiType: notiType,
            notiMessage: notiMessage,
            sendAt: new Date().toISOString()
        };
        
        try {
            console.log(`Sending notification to account ID: ${accountId}`, requestBody);
            
            const response = await fetch(`https://localhost:7009/api/Notification/CreateAndSendToAccount/${accountId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'accept': '*/*'
                },
                body: JSON.stringify(requestBody)
            });
            
            if (response.ok) {
                console.log('Notification sent successfully to account:', accountId);
                return true;
            } else {
                const errorData = await response.text();
                console.error('Failed to send notification:', errorData);
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error sending notification to account:', error);
            throw error;
        }
    }
}

// Export for use in other modules
window.AccountManager = AccountManager;
