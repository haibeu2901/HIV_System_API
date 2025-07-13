// Check authentication on page load
document.addEventListener('DOMContentLoaded', function() {
    checkAuth();
    loadDashboardData();
    
    // Initialize event listeners
    initializeEventListeners();
});

// Authentication check
function checkAuth() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/public-view/landingpage.html';
        return;
    }
    
    // Verify token is valid and user is admin
    // You can add additional verification here
}

// Logout function
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('accId');
    window.location.href = '/public-view/landingpage.html';
}

// Section navigation
function showSection(sectionId) {
    // Hide all sections
    document.querySelectorAll('.admin-section').forEach(section => {
        section.classList.remove('active');
    });
    
    // Remove active class from all nav links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    
    // Show selected section
    const targetSection = document.getElementById(sectionId);
    if (targetSection) {
        targetSection.classList.add('active');
    }
    
    // Add active class to clicked nav link using data attribute
    const activeLink = document.querySelector(`[data-section="${sectionId}"]`);
    if (activeLink) {
        activeLink.classList.add('active');
    }
    
    // Load section-specific data
    loadSectionData(sectionId);
}

// Load section-specific data
function loadSectionData(sectionId) {
    switch(sectionId) {
        case 'dashboard':
            loadDashboardData();
            break;
        case 'notifications':
            loadNotifications();
            break;
        case 'appointments':
            loadAppointments();
            break;
        case 'medical-records':
            loadMedicalRecords();
            break;
        case 'accounts':
            loadAccounts();
            break;
    }
}

// Dashboard Data
async function loadDashboardData() {
    const token = localStorage.getItem('token');
    
    try {
        // Load statistics
        await Promise.all([
            loadPatientCount(),
            loadDoctorCount(),
            loadAppointmentCount(),
            loadRecentActivity()
        ]);
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

async function loadPatientCount() {
    const token = localStorage.getItem('token');
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

async function loadDoctorCount() {
    const token = localStorage.getItem('token');
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

async function loadAppointmentCount() {
    const token = localStorage.getItem('token');
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

async function loadRecentActivity() {
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
                <i class="fas fa-${getActivityIcon(activity.type)}"></i>
            </div>
            <div class="activity-content">
                <div class="activity-title">${activity.title}</div>
                <div class="activity-time">${activity.time}</div>
            </div>
        </div>
    `).join('');
}

function getActivityIcon(type) {
    const icons = {
        appointment: 'calendar-plus',
        patient: 'user-plus',
        doctor: 'user-md',
        system: 'cog'
    };
    return icons[type] || 'info-circle';
}

// Notifications
async function loadNotifications() {
    const notificationsList = document.getElementById('notifications-list');
    notificationsList.innerHTML = '<div class="loader"></div>';
    
    // Mock notifications data
    setTimeout(() => {
        const notifications = [
            { id: 1, title: 'System Update', message: 'System will be updated tonight', type: 'system', created: '2024-01-15' },
            { id: 2, title: 'Appointment Reminder', message: 'Multiple appointments scheduled for tomorrow', type: 'appointment', created: '2024-01-14' },
            { id: 3, title: 'Monthly Report', message: 'Monthly report is ready for review', type: 'reminder', created: '2024-01-13' }
        ];
        
        notificationsList.innerHTML = notifications.map(notification => `
            <div class="list-item">
                <div class="item-content">
                    <div class="item-title">${notification.title}</div>
                    <div class="item-subtitle">${notification.message}</div>
                    <small>Created: ${notification.created}</small>
                </div>
                <div class="item-actions">
                    <button class="btn-small btn-edit" onclick="editNotification(${notification.id})">Edit</button>
                    <button class="btn-small btn-delete" onclick="deleteNotification(${notification.id})">Delete</button>
                </div>
            </div>
        `).join('');
    }, 1000);
}

// Appointments
async function loadAppointments() {
    const appointmentsList = document.getElementById('appointments-list');
    appointmentsList.innerHTML = '<div class="loader"></div>';
    
    const token = localStorage.getItem('token');
    
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
                appointmentsList.innerHTML = '<div class="no-data">No appointments found</div>';
                return;
            }
            
            // Clear the list
            appointmentsList.innerHTML = '';
            
            // Create elements dynamically
            appointments.forEach(appointment => {
                const listItem = document.createElement('div');
                listItem.className = 'list-item';
                
                const itemContent = document.createElement('div');
                itemContent.className = 'item-content';
                
                const itemTitle = document.createElement('div');
                itemTitle.className = 'item-title';
                itemTitle.textContent = `Appointment #${appointment.appointmentId}`;
                
                const itemSubtitle = document.createElement('div');
                itemSubtitle.className = 'item-subtitle';
                itemSubtitle.textContent = `Patient: ${appointment.patientName} | Doctor: ${appointment.doctorName}`;
                
                const appointmentDetails = document.createElement('div');
                appointmentDetails.className = 'appointment-details';
                appointmentDetails.innerHTML = `
                    <small><strong>Date:</strong> ${formatDate(appointment.apmtDate)}</small><br>
                    <small><strong>Time:</strong> ${appointment.apmTime}</small><br>
                    <small><strong>Notes:</strong> ${appointment.notes || 'No notes available'}</small><br>
                    <small><strong>Patient ID:</strong> ${appointment.patientId} | <strong>Doctor ID:</strong> ${appointment.doctorId}</small>
                `;
                
                itemContent.appendChild(itemTitle);
                itemContent.appendChild(itemSubtitle);
                itemContent.appendChild(appointmentDetails);
                
                const itemActions = document.createElement('div');
                itemActions.className = 'item-actions';
                
                // Status badge
                const statusBadge = document.createElement('span');
                statusBadge.className = `status-badge status-${getStatusClassFromNumber(appointment.apmStatus)}`;
                statusBadge.textContent = getStatusTextFromNumber(appointment.apmStatus);
                itemActions.appendChild(statusBadge);
                
                // Action buttons
                const viewBtn = createAppointmentButton(appointment.appointmentId, 'view', 'View', 'btn-info');
                const editBtn = createAppointmentButton(appointment.appointmentId, 'edit', 'Edit', 'btn-edit');
                const deleteBtn = createAppointmentButton(appointment.appointmentId, 'delete', 'Delete', 'btn-delete');
                
                itemActions.appendChild(viewBtn);
                itemActions.appendChild(editBtn);
                itemActions.appendChild(deleteBtn);
                
                listItem.appendChild(itemContent);
                listItem.appendChild(itemActions);
                
                appointmentsList.appendChild(listItem);
            });
            
            // Add create button after loading appointments
            addCreateAppointmentButton();
            
        } else {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error('Error loading appointments:', error);
        appointmentsList.innerHTML = '<div class="error-message">Error loading appointments. Please try again.</div>';
    }
}

// CRUD Operations for Appointments

// Get Appointment by ID
async function getAppointmentById(id) {
    const token = localStorage.getItem('token');
    
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

// Create Appointment
async function createAppointment(appointmentData) {
    const token = localStorage.getItem('token');
    
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

// Update Appointment
async function updateAppointment(id, appointmentData) {
    const token = localStorage.getItem('token');
    
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

// Delete Appointment
async function deleteAppointment(id) {
    const token = localStorage.getItem('token');
    
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

// Updated Dynamic button creation for appointments
function createAppointmentButton(appointmentId, action, text, className) {
    const button = document.createElement('button');
    button.className = `btn-small ${className}`;
    button.textContent = text;
    
    switch(action) {
        case 'view':
            button.addEventListener('click', () => viewAppointment(appointmentId));
            break;
        case 'edit':
            button.addEventListener('click', () => editAppointment(appointmentId));
            break;
        case 'delete':
            button.addEventListener('click', () => deleteAppointmentConfirm(appointmentId));
            break;
    }
    
    return button;
}

// View Appointment Function
async function viewAppointment(appointmentId) {
    try {
        const appointment = await getAppointmentById(appointmentId);
        showAppointmentModal(appointment);
    } catch (error) {
        console.error('Error viewing appointment:', error);
        alert('Error loading appointment details');
    }
}

// Show Appointment Modal
function showAppointmentModal(appointment) {
    // Create modal if it doesn't exist
    let modal = document.getElementById('appointmentModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'appointmentModal';
        modal.className = 'modal';
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Appointment Details</h3>
                    <button class="close-btn" onclick="closeModal('appointmentModal')">&times;</button>
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
                <h4><i class="fas fa-calendar-alt"></i> Appointment Information</h4>
                <p><strong>Appointment ID:</strong> ${appointment.appointmentId}</p>
                <p><strong>Date:</strong> ${formatDate(appointment.apmtDate)}</p>
                <p><strong>Time:</strong> ${appointment.apmTime}</p>
                <p><strong>Status:</strong> <span class="status-badge status-${getStatusClassFromNumber(appointment.apmStatus)}">${getStatusTextFromNumber(appointment.apmStatus)}</span></p>
            </div>
            
            <div class="detail-section">
                <h4><i class="fas fa-user"></i> Patient Information</h4>
                <p><strong>Name:</strong> ${appointment.patientName}</p>
                <p><strong>Patient ID:</strong> ${appointment.patientId}</p>
            </div>
            
            <div class="detail-section">
                <h4><i class="fas fa-user-md"></i> Doctor Information</h4>
                <p><strong>Name:</strong> ${appointment.doctorName}</p>
                <p><strong>Doctor ID:</strong> ${appointment.doctorId}</p>
            </div>
            
            <div class="detail-section">
                <h4><i class="fas fa-file-medical-alt"></i> Notes</h4>
                <div class="notes-content">
                    ${appointment.notes || 'No notes available for this appointment.'}
                </div>
            </div>
        </div>
    `;
    
    // Show modal
    modal.style.display = 'block';
}

// Edit Appointment Function
async function editAppointment(appointmentId) {
    try {
        const appointment = await getAppointmentById(appointmentId);
        showEditAppointmentModal(appointment);
    } catch (error) {
        console.error('Error editing appointment:', error);
        alert('Error loading appointment for editing');
    }
}

// Show Edit Appointment Modal
function showEditAppointmentModal(appointment) {
    // Create modal if it doesn't exist
    let modal = document.getElementById('editAppointmentModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'editAppointmentModal';
        modal.className = 'modal';
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Edit Appointment</h3>
                    <button class="close-btn" onclick="closeModal('editAppointmentModal')">&times;</button>
                </div>
                <div class="modal-body">
                    <form id="editAppointmentForm">
                        <input type="hidden" id="edit-appointment-id" value="${appointment.appointmentId}">
                        
                        <div class="form-group">
                            <label for="edit-patient-id">Patient ID</label>
                            <input type="number" id="edit-patient-id" value="${appointment.patientId}" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="edit-doctor-id">Doctor ID</label>
                            <input type="number" id="edit-doctor-id" value="${appointment.doctorId}" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="edit-appointment-date">Date</label>
                            <input type="date" id="edit-appointment-date" value="${appointment.apmtDate}" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="edit-appointment-time">Time</label>
                            <input type="time" id="edit-appointment-time" value="${appointment.apmTime}" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="edit-appointment-status">Status</label>
                            <select id="edit-appointment-status" required>
                                <option value="1"${appointment.apmStatus === 1 ? ' selected' : ''}>Pending</option>
                                <option value="2"${appointment.apmStatus === 2 ? ' selected' : ''}>Confirmed</option>
                                <option value="3"${appointment.apmStatus === 3 ? ' selected' : ''}>Completed</option>
                                <option value="4"${appointment.apmStatus === 4 ? ' selected' : ''}>Cancelled</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label for="edit-appointment-notes">Notes</label>
                            <textarea id="edit-appointment-notes" rows="4">${appointment.notes || ''}</textarea>
                        </div>
                        
                        <div class="form-actions">
                            <button type="button" onclick="closeModal('editAppointmentModal')">Cancel</button>
                            <button type="submit">Update Appointment</button>
                        </div>
                    </form>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    } else {
        // If modal exists, just update the form values
        document.getElementById('edit-appointment-id').value = appointment.appointmentId;
        document.getElementById('edit-patient-id').value = appointment.patientId;
        document.getElementById('edit-doctor-id').value = appointment.doctorId;
        document.getElementById('edit-appointment-date').value = appointment.apmtDate;
        document.getElementById('edit-appointment-time').value = appointment.apmTime;
        document.getElementById('edit-appointment-status').value = appointment.apmStatus;
        document.getElementById('edit-appointment-notes').value = appointment.notes || '';
    }
    
    // Show modal
    modal.style.display = 'block';
}

// Show Create Appointment Modal
function showCreateAppointmentModal() {
    // Create modal if it doesn't exist
    let modal = document.getElementById('createAppointmentModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'createAppointmentModal';
        modal.className = 'modal';
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Create New Appointment</h3>
                    <button class="close-btn" onclick="closeModal('createAppointmentModal')">&times;</button>
                </div>
                <div class="modal-body">
                    <form id="createAppointmentForm">
                        <div class="form-group">
                            <label for="patient-id">Patient ID</label>
                            <input type="number" id="patient-id" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="doctor-id">Doctor ID</label>
                            <input type="number" id="doctor-id" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="appointment-date">Date</label>
                            <input type="date" id="appointment-date" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="appointment-time">Time</label>
                            <input type="time" id="appointment-time" required>
                        </div>
                        
                        <div class="form-group">
                            <label for="appointment-status">Status</label>
                            <select id="appointment-status" required>
                                <option value="1">Pending</option>
                                <option value="2">Confirmed</option>
                                <option value="3">Completed</option>
                                <option value="4">Cancelled</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label for="appointment-notes">Notes</label>
                            <textarea id="appointment-notes" rows="4"></textarea>
                        </div>
                        
                        <div class="form-actions">
                            <button type="button" onclick="closeModal('createAppointmentModal')">Cancel</button>
                            <button type="submit">Create Appointment</button>
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

// Delete Appointment Confirmation
async function deleteAppointmentConfirm(appointmentId) {
    if (confirm('Are you sure you want to delete this appointment? This action cannot be undone.')) {
        try {
            const submitButton = event.target;
            showButtonLoader(submitButton, 'Deleting...');
            
            await deleteAppointment(appointmentId);
            
            // Show success message
            alert('Appointment deleted successfully!');
            
            // Reload the appointments list
            loadAppointments();
            
        } catch (error) {
            console.error('Error deleting appointment:', error);
            alert('Error deleting appointment. Please try again.');
        } finally {
            if (event.target) {
                hideButtonLoader(event.target);
            }
        }
    }
}

// Add Create Appointment Button
function addCreateAppointmentButton() {
    const sectionHeader = document.querySelector('#appointments .section-header');
    if (sectionHeader && !sectionHeader.querySelector('.create-appointment-btn')) {
        const createButton = document.createElement('button');
        createButton.className = 'btn-primary create-appointment-btn';
        createButton.innerHTML = '<i class="fas fa-plus"></i> Create Appointment';
        createButton.onclick = showCreateAppointmentModal;
        sectionHeader.appendChild(createButton);
    }
}

// Enhanced Filter Appointments Function
function filterAppointments() {
    const dateFilter = document.getElementById('appointment-date-filter').value;
    const statusFilter = document.getElementById('appointment-status-filter').value;
    
    const appointmentsList = document.getElementById('appointments-list');
    appointmentsList.innerHTML = '<div class="loader"></div>';
    
    const token = localStorage.getItem('token');
    
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
            appointmentsList.innerHTML = '<div class="no-data">No appointments found matching your filters</div>';
            return;
        }
        
        // Clear and populate with filtered results
        appointmentsList.innerHTML = '';
        filteredAppointments.forEach(appointment => {
            const listItem = document.createElement('div');
            listItem.className = 'list-item';
            
            const itemContent = document.createElement('div');
            itemContent.className = 'item-content';
            
            const itemTitle = document.createElement('div');
            itemTitle.className = 'item-title';
            itemTitle.textContent = `Appointment #${appointment.appointmentId}`;
            
            const itemSubtitle = document.createElement('div');
            itemSubtitle.className = 'item-subtitle';
            itemSubtitle.textContent = `Patient: ${appointment.patientName} | Doctor: ${appointment.doctorName}`;
            
            const appointmentDetails = document.createElement('div');
            appointmentDetails.className = 'appointment-details';
            appointmentDetails.innerHTML = `
                <small><strong>Date:</strong> ${formatDate(appointment.apmtDate)}</small><br>
                <small><strong>Time:</strong> ${appointment.apmTime}</small><br>
                <small><strong>Notes:</strong> ${appointment.notes || 'No notes available'}</small><br>
                <small><strong>Patient ID:</strong> ${appointment.patientId} | <strong>Doctor ID:</strong> ${appointment.doctorId}</small>
            `;
            
            itemContent.appendChild(itemTitle);
            itemContent.appendChild(itemSubtitle);
            itemContent.appendChild(appointmentDetails);
            
            const itemActions = document.createElement('div');
            itemActions.className = 'item-actions';
            
            // Status badge
            const statusBadge = document.createElement('span');
            statusBadge.className = `status-badge status-${getStatusClassFromNumber(appointment.apmStatus)}`;
            statusBadge.textContent = getStatusTextFromNumber(appointment.apmStatus);
            itemActions.appendChild(statusBadge);
            
            // Action buttons
            const viewBtn = createAppointmentButton(appointment.appointmentId, 'view', 'View', 'btn-info');
            const editBtn = createAppointmentButton(appointment.appointmentId, 'edit', 'Edit', 'btn-edit');
            const deleteBtn = createAppointmentButton(appointment.appointmentId, 'delete', 'Delete', 'btn-delete');
            
            itemActions.appendChild(viewBtn);
            itemActions.appendChild(editBtn);
            itemActions.appendChild(deleteBtn);
            
            listItem.appendChild(itemContent);
            listItem.appendChild(itemActions);
            
            appointmentsList.appendChild(listItem);
        });
    })
    .catch(error => {
        console.error('Error filtering appointments:', error);
        appointmentsList.innerHTML = '<div class="error-message">Error filtering appointments. Please try again.</div>';
    });
}

// Add form submission handlers for appointments
document.addEventListener('DOMContentLoaded', function() {
    // Add create button when page loads
    setTimeout(addCreateAppointmentButton, 1000);
    
    // Create Appointment Form Handler
    document.addEventListener('submit', function(e) {
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
            showButtonLoader(submitButton, 'Creating...');
            
            createAppointment(formData)
                .then(() => {
                    alert('Appointment created successfully!');
                    closeModal('createAppointmentModal');
                    loadAppointments();
                })
                .catch(error => {
                    console.error('Error creating appointment:', error);
                    alert('Error creating appointment. Please try again.');
                })
                .finally(() => {
                    hideButtonLoader(submitButton);
                });
        }
    });
    
    // Edit Appointment Form Handler
    document.addEventListener('submit', function(e) {
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
            showButtonLoader(submitButton, 'Updating...');
            
            updateAppointment(appointmentId, formData)
                .then(() => {
                    alert('Appointment updated successfully!');
                    closeModal('editAppointmentModal');
                    loadAppointments();
                })
                .catch(error => {
                    console.error('Error updating appointment:', error);
                    alert('Error updating appointment. Please try again.');
                })
                .finally(() => {
                    hideButtonLoader(submitButton);
                });
        }
    });
});

// Initialize all event listeners
function initializeEventListeners() {
    // Logout button
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', logout);
    }
    
    // Navigation links
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const section = this.getAttribute('data-section');
            showSection(section);
        });
    });
    
    // Create buttons
    const createNotificationBtn = document.getElementById('create-notification-btn');
    if (createNotificationBtn) {
        createNotificationBtn.addEventListener('click', showCreateNotificationModal);
    }
    
    const createAccountBtn = document.getElementById('create-account-btn');
    if (createAccountBtn) {
        createAccountBtn.addEventListener('click', showCreateAccountModal);
    }
    
    // Filter elements
    const notificationFilter = document.getElementById('notification-filter');
    if (notificationFilter) {
        notificationFilter.addEventListener('change', filterNotifications);
    }
    
    const appointmentDateFilter = document.getElementById('appointment-date-filter');
    if (appointmentDateFilter) {
        appointmentDateFilter.addEventListener('change', filterAppointments);
    }
    
    const appointmentStatusFilter = document.getElementById('appointment-status-filter');
    if (appointmentStatusFilter) {
        appointmentStatusFilter.addEventListener('change', filterAppointments);
    }
    
    const accountRoleFilter = document.getElementById('account-role-filter');
    if (accountRoleFilter) {
        accountRoleFilter.addEventListener('change', filterAccounts);
    }
    
    // Search functionality
    const medicalSearchBtn = document.getElementById('medical-search-btn');
    if (medicalSearchBtn) {
        medicalSearchBtn.addEventListener('click', searchMedicalRecords);
    }
    
    const medicalSearchInput = document.getElementById('medical-search');
    if (medicalSearchInput) {
        medicalSearchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                searchMedicalRecords();
            }
        });
    }
    
    const accountSearchInput = document.getElementById('account-search');
    if (accountSearchInput) {
        accountSearchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                filterAccounts();
            }
        });
    }
    
    // Modal close buttons
    const closeButtons = document.querySelectorAll('.close-btn, button[data-modal]');
    closeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const modalId = this.getAttribute('data-modal');
            if (modalId) {
                closeModal(modalId);
            }
        });
    });
    
    // Form submissions
    const createNotificationForm = document.getElementById('createNotificationForm');
    if (createNotificationForm) {
        createNotificationForm.addEventListener('submit', handleCreateNotification);
    }
    
    const createAccountForm = document.getElementById('createAccountForm');
    if (createAccountForm) {
        createAccountForm.addEventListener('submit', handleCreateAccount);
    }
    
    // Close modals when clicking outside
    window.addEventListener('click', function(event) {
        const modals = document.querySelectorAll('.modal');
        modals.forEach(modal => {
            if (event.target === modal) {
                closeModal(modal.id);
            }
        });
    });
    
    // Close modals with Escape key
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            const openModal = document.querySelector('.modal[style*="block"]');
            if (openModal) {
                closeModal(openModal.id);
            }
        }
    });
}

// Helper function to format date
function formatDate(dateString) {
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

// Helper function to get status class from number
function getStatusClassFromNumber(statusNumber) {
    switch (statusNumber) {
        case 1:
            return 'pending';
        case 2:
            return 'confirmed';
        case 3:
            return 'completed';
        case 4:
            return 'cancelled';
        default:
            return 'pending';
    }
}

// Helper function to get status text from number
function getStatusTextFromNumber(statusNumber) {
    switch (statusNumber) {
        case 1:
            return 'Pending';
        case 2:
            return 'Confirmed';
        case 3:
            return 'Completed';
        case 4:
            return 'Cancelled';
        default:
            return 'Unknown';
    }
}

// Close Modal Function
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        
        // Clear form data when closing modal
        const form = modal.querySelector('form');
        if (form) {
            form.reset();
        }
    }
}

// Show Create Notification Modal
function showCreateNotificationModal() {
    document.getElementById('createNotificationModal').style.display = 'block';
}

// Show Create Account Modal
function showCreateAccountModal() {
    document.getElementById('createAccountModal').style.display = 'block';
}

// Filter Notifications
function filterNotifications() {
    const filter = document.getElementById('notification-filter').value;
    console.log('Filtering notifications by:', filter);
    loadNotifications();
}

// Filter Accounts
function filterAccounts() {
    const roleFilter = document.getElementById('account-role-filter').value;
    const searchTerm = document.getElementById('account-search').value;
    console.log('Filtering accounts by:', roleFilter, searchTerm);
    loadAccounts();
}

// Edit Notification
function editNotification(id) {
    console.log('Edit notification:', id);
    alert('Edit notification functionality will be implemented here');
}

// Delete Notification
function deleteNotification(id) {
    if (confirm('Are you sure you want to delete this notification?')) {
        console.log('Delete notification:', id);
        loadNotifications();
    }
}

// Edit Account
function editAccount(id) {
    console.log('Edit account:', id);
    alert('Edit account functionality will be implemented here');
}

// Delete Account
function deleteAccount(id) {
    if (confirm('Are you sure you want to delete this account?')) {
        console.log('Delete account:', id);
        loadAccounts();
    }
}

// Handle Create Notification Form
function handleCreateNotification(e) {
    e.preventDefault();
    
    const title = document.getElementById('notification-title').value;
    const message = document.getElementById('notification-message').value;
    const type = document.getElementById('notification-type').value;
    
    console.log('Creating notification:', { title, message, type });
    
    alert('Notification created successfully!');
    closeModal('createNotificationModal');
    loadNotifications();
}

// Handle Create Account Form
function handleCreateAccount(e) {
    e.preventDefault();
    
    const username = document.getElementById('account-username').value;
    const email = document.getElementById('account-email').value;
    const fullname = document.getElementById('account-fullname').value;
    const role = document.getElementById('account-role').value;
    const password = document.getElementById('account-password').value;
    
    console.log('Creating account:', { username, email, fullname, role, password });
    
    alert('Account created successfully!');
    closeModal('createAccountModal');
    loadAccounts();
}

// Load Medical Records
async function loadMedicalRecords() {
    const recordsList = document.getElementById('medical-records-list');
    recordsList.innerHTML = '<div class="loader"></div>';
    
    const token = localStorage.getItem('token');
    
    try {
        const response = await fetch('https://localhost:7009/api/PatientMedicalRecord/GetPatientsMedicalRecord', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            console.log('Medical records data:', data);
            
            if (!data || data.length === 0) {
                recordsList.innerHTML = '<div class="no-data">No medical records found</div>';
                return;
            }
            
            recordsList.innerHTML = '<div class="no-data">Medical records loaded successfully</div>';
            
        } else {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error('Error loading medical records:', error);
        recordsList.innerHTML = '<div class="error-message">Error loading medical records. Please try again.</div>';
    }
}

// Search Medical Records
function searchMedicalRecords() {
    const searchTerm = document.getElementById('medical-search').value;
    console.log('Searching medical records:', searchTerm);
    loadMedicalRecords();
}

// Load Accounts
async function loadAccounts() {
    const accountsList = document.getElementById('accounts-list');
    accountsList.innerHTML = '<div class="loader"></div>';
    
    setTimeout(() => {
        accountsList.innerHTML = '<div class="no-data">Accounts loaded successfully</div>';
    }, 1000);
}

// Button Loader Functions
function showButtonLoader(button, text = 'Loading...') {
    button.disabled = true;
    button.classList.add('btn-loading');
    button.dataset.originalText = button.textContent;
    button.innerHTML = `
        <span style="opacity: 0;">${button.dataset.originalText}</span>
        <div class="small-loader" style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);"></div>
    `;
}

function hideButtonLoader(button) {
    button.disabled = false;
    button.classList.remove('btn-loading');
    button.innerHTML = button.dataset.originalText || 'Submit';
}