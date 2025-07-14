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
            const modalHTML = `
            <div id="patientProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 600px;">
            </div>
            <div id="doctorProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 800px;">
            </div>
            <div id="generalProfileModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 600px;">
                </div>
            </div>
        `;
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
                    accountsList.innerHTML = '<div class="no-data">No accounts found</div>';
                    return;
                }
                
                this.renderAccounts(data);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error loading accounts:', error);
            accountsList.innerHTML = '<div class="error-message">Error loading accounts. Please try again.</div>';
        }
    }

    // Show create account modal
    showCreateAccountModal() {
        const modal = document.getElementById('createAccountModal');
        if (modal) {
            modal.style.display = 'block';
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
                    accountsList.innerHTML = '<div class="no-data">No accounts found matching the criteria</div>';
                    return;
                }
                
                this.renderAccounts(data);
                
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error filtering accounts:', error);
            accountsList.innerHTML = '<div class="error-message">Error filtering accounts. Please try again.</div>';
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
            window.utils.showToast('Error loading account details. Please try again.', 'error');
        }
    }

    // Delete account
    async deleteAccount(id) {
        if (confirm('Are you sure you want to delete this account? This action cannot be undone.')) {
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
                    window.utils.showToast('Account deleted successfully!', 'success');
                    this.loadAccounts(); // Reload the accounts list
                } else {
                    const errorData = await response.text();
                    throw new Error(errorData || `HTTP error! status: ${response.status}`);
                }
            } catch (error) {
                console.error('Error deleting account:', error);
                window.utils.showToast('Error deleting account. Please try again.', 'error');
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
                            <span>Username: ${account.accUsername}</span>
                        </div>
                        <div class="detail-item">
                            <i class="fas fa-shield-alt"></i>
                            <span>Role: ${roleName}</span>
                        </div>
                    </div>
                    
                    <div class="account-actions">
                        <button class="btn-secondary" onclick="accountManager.viewAccount(${account.accId})">
                            <i class="fas fa-eye"></i> View
                        </button>
                        <button class="btn-primary" onclick="accountManager.editAccount(${account.accId})">
                            <i class="fas fa-edit"></i> Edit
                        </button>
                        ${userRole !== 'admin' ? `
                            <button class="btn-danger" onclick="accountManager.deleteAccount(${account.accId})">
                                <i class="fas fa-trash"></i> Delete
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
            'admin': 'Administrator',
            'supervisor': 'Supervisor',
            'doctor': 'Doctor',
            'staff': 'Staff',
            'patient': 'Patient'
        };
        return roleNames[role] || 'Unknown';
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
            window.utils.showToast('Error loading account details. Please try again.', 'error');
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
            window.utils.showToast('Error loading patient profile. Please try again.', 'error');
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
            window.utils.showToast('Error loading doctor profile. Please try again.', 'error');
        }
    }

    // View general profile (for admin, staff, supervisor)
    viewGeneralProfile(account) {
        this.showGeneralProfileModal(account);
    }

    // Show patient profile modal
    showPatientProfileModal(patientData) {
        const modalHTML = `
            <div id="patientProfileModal" class="modal" style="display: block;">
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
                                    <span class="role-badge role-patient">Patient</span>
                                </div>
                            </div>
                            
                            <div class="profile-details">
                                <div class="detail-section">
                                    <h4><i class="fas fa-info-circle"></i> Account Information</h4>
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
                                            <label>Username:</label>
                                            <span>${patientData.accUsername}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-user-md"></i> Patient Details</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Patient ID:</label>
                                            <span>${patientData.ptnId || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Phone:</label>
                                            <span>${patientData.ptnPhone || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Address:</label>
                                            <span>${patientData.ptnAddress || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Date of Birth:</label>
                                            <span>${patientData.ptnDateOfBirth ? new Date(patientData.ptnDateOfBirth).toLocaleDateString() : 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Gender:</label>
                                            <span>${patientData.ptnGender || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Emergency Contact:</label>
                                            <span>${patientData.ptnEmergencyContact || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-notes-medical"></i> Medical Information</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Insurance Number:</label>
                                            <span>${patientData.ptnInsuranceNumber || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Medical Notes:</label>
                                            <span>${patientData.ptnMedicalNotes || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Close</button>
                        <button class="btn-primary" onclick="accountManager.editPatientProfile(${patientData.accId})">
                            <i class="fas fa-edit"></i> Edit Profile
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
            <div id="doctorProfileModal" class="modal" style="display: block;">
                <div class="modal-content" style="max-width: 800px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-user-md"></i> Doctor Profile</h3>
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
                                    <span class="role-badge role-doctor">Doctor</span>
                                </div>
                            </div>
                            
                            <div class="profile-details">
                                <div class="detail-section">
                                    <h4><i class="fas fa-info-circle"></i> Account Information</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Account ID:</label>
                                            <span>${doctorData.accId}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Email:</label>
                                            <span>${doctorData.email}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Username:</label>
                                            <span>${doctorData.accUsername}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-stethoscope"></i> Doctor Details</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Doctor ID:</label>
                                            <span>${doctorData.docId || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Phone:</label>
                                            <span>${doctorData.docPhone || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Address:</label>
                                            <span>${doctorData.docAddress || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Date of Birth:</label>
                                            <span>${doctorData.docDateOfBirth ? new Date(doctorData.docDateOfBirth).toLocaleDateString() : 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Gender:</label>
                                            <span>${doctorData.docGender || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Specialization:</label>
                                            <span>${doctorData.docSpecialization || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="detail-section">
                                    <h4><i class="fas fa-certificate"></i> Professional Information</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>License Number:</label>
                                            <span>${doctorData.docLicenseNumber || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Years of Experience:</label>
                                            <span>${doctorData.docYearsOfExperience || 'N/A'}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Qualification:</label>
                                            <span>${doctorData.docQualification || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Close</button>
                        <button class="btn-primary" onclick="accountManager.editDoctorProfile(${doctorData.accId})">
                            <i class="fas fa-edit"></i> Edit Profile
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
            <div id="generalProfileModal" class="modal" style="display: block;">
                <div class="modal-content" style="max-width: 600px;">
                    <div class="modal-header">
                        <h3><i class="fas ${this.getRoleIcon(userRole)}"></i> ${roleName} Profile</h3>
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
                                    <h4><i class="fas fa-info-circle"></i> Account Information</h4>
                                    <div class="detail-grid">
                                        <div class="detail-item">
                                            <label>Account ID:</label>
                                            <span>${account.accId}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Email:</label>
                                            <span>${account.email}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Username:</label>
                                            <span>${account.accUsername}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Full Name:</label>
                                            <span>${account.fullname}</span>
                                        </div>
                                        <div class="detail-item">
                                            <label>Role:</label>
                                            <span>${roleName}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn-secondary" onclick="accountManager.closeProfileModal()">Close</button>
                        <button class="btn-primary" onclick="accountManager.editGeneralProfile(${account.accId})">
                            <i class="fas fa-edit"></i> Edit Profile
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
        window.utils.showToast('Edit patient profile functionality coming soon!', 'info');
    }

    // Edit doctor profile
    editDoctorProfile(accId) {
        console.log('Edit doctor profile:', accId);
        // Implementation for editing doctor profile
        window.utils.showToast('Edit doctor profile functionality coming soon!', 'info');
    }

    // Edit general profile
    editGeneralProfile(accId) {
        console.log('Edit general profile:', accId);
        // Implementation for editing general profile
        window.utils.showToast('Edit profile functionality coming soon!', 'info');
    }

    // Show edit account modal
    showEditAccountModal(account) {
        const userRole = this.detectUserRole(account.accUsername);
        const roleName = this.getRoleDisplayName(userRole);
        
        const modalHTML = `
            <div id="editAccountModal" class="modal" style="display: flex;">
                <div class="modal-content" style="max-width: 600px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-edit"></i> Edit Account</h3>
                        <button class="close-btn" onclick="accountManager.closeEditModal()">&times;</button>
                    </div>
                    <div class="modal-body">
                        <form id="editAccountForm">
                            <input type="hidden" id="edit-account-id" value="${account.accId}">
                            
                            <div class="form-group">
                                <label for="edit-account-password">New Password</label>
                                <input type="password" id="edit-account-password" placeholder="Enter new password (optional)" value="">
                                <small class="form-text">Leave blank to keep current password</small>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-email">Email</label>
                                <input type="email" id="edit-account-email" value="${account.email}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-fullname">Full Name</label>
                                <input type="text" id="edit-account-fullname" value="${account.fullname}" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-dob">Date of Birth</label>
                                <input type="date" id="edit-account-dob" value="${account.dob ? account.dob.split('T')[0] : ''}">
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-gender">Gender</label>
                                <select id="edit-account-gender">
                                    <option value="">Select Gender</option>
                                    <option value="true" ${account.gender === true ? 'selected' : ''}>Male</option>
                                    <option value="false" ${account.gender === false ? 'selected' : ''}>Female</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-roles">Role</label>
                                <select id="edit-account-roles">
                                    <option value="0" ${userRole === 'admin' ? 'selected' : ''}>Administrator</option>
                                    <option value="1" ${userRole === 'doctor' ? 'selected' : ''}>Doctor</option>
                                    <option value="2" ${userRole === 'patient' ? 'selected' : ''}>Patient</option>
                                    <option value="3" ${userRole === 'staff' ? 'selected' : ''}>Staff</option>
                                    <option value="4" ${userRole === 'supervisor' ? 'selected' : ''}>Supervisor</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="edit-account-active">Account Status</label>
                                <select id="edit-account-active">
                                    <option value="true" ${account.isActive !== false ? 'selected' : ''}>Active</option>
                                    <option value="false" ${account.isActive === false ? 'selected' : ''}>Inactive</option>
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
        
        const token = this.authManager.getToken();
        
        // Prepare the request body according to the API specification
        const requestBody = {
            email: email,
            fullname: fullname,
            dob: dob || "2025-07-13", // Default date if not provided
            gender: gender !== "" ? gender === "true" : true, // Convert to boolean
            roles: parseInt(roles),
            isActive: isActive
        };
        
        // Only include password if provided
        if (password && password.trim() !== '') {
            requestBody.accPassword = password;
        }
        
        try {
            window.utils.showButtonLoader(e.target.querySelector('button[type="submit"]'), true);
            
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
                window.utils.showToast('Account updated successfully!', 'success');
                this.closeEditModal();
                this.loadAccounts(); // Reload the accounts list
            } else {
                const errorData = await response.text();
                throw new Error(errorData || `HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error updating account:', error);
            window.utils.showToast('Error updating account. Please try again.', 'error');
        } finally {
            window.utils.showButtonLoader(e.target.querySelector('button[type="submit"]'), false);
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
}

// Export for use in other modules
window.AccountManager = AccountManager;
