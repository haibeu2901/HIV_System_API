const params = new URLSearchParams(window.location.search);
const selectedId = params.get('id');

document.addEventListener("DOMContentLoaded", async () => {
  const token = localStorage.getItem("token");
  const calendarEl = document.getElementById("calendar");

  let events = [];


  try {
    const res = await fetch(
      "https://localhost:7009/api/Appointment/GetAllPersonalAppointments",
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    if (!res.ok) throw new Error("Failed to fetch appointments");
    const data = await res.json();

    events = data.map((appt) => ({
      title: `Dr. ${appt.doctorName}\n${appt.apmTime} (${renderStatusText(
        appt.apmStatus
      )})`,
      start: `${appt.apmtDate}T${appt.apmTime}`,
      end: `${appt.apmtDate}T${addMinutes(appt.apmTime, 30)}`, // 30 min duration
      color: getStatusColor(appt.apmStatus),
      extendedProps: {
        appointmentId: appt.appointmentId,
        notes: appt.notes || "None",
        status: renderStatusText(appt.apmStatus),
        statusCode: appt.apmStatus,
      },
    }))
    .filter(event => event.extendedProps.statusCode !== 3 && event.extendedProps.statusCode !== 4); // Remove cancelled appointments
  } catch (err) {
    calendarEl.innerHTML = `<p style="color:red;">${err.message}</p>`;
    return;
  }

  const calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "dayGridMonth",
    headerToolbar: {
      left: "prev,next today",
      center: "title",
      right: "dayGridMonth,timeGridWeek,timeGridDay",
    },
    events: events,
    eventDisplay: "block",
    eventTimeFormat: {
      hour: "2-digit",
      minute: "2-digit",
      meridiem: false,
      hour12: false,
    },
    displayEventTime: false,
    eventDidMount: function (info) {
      // Make events look like sticky notes
      info.el.style.borderRadius = "12px";
      info.el.style.boxShadow = "0 2px 8px rgba(44,62,80,0.10)";
      info.el.style.fontWeight = "500";
      info.el.style.whiteSpace = "pre-line";
    },
    eventClick: function (info) {
      // Store current appointment data globally for update functionality
      window.currentAppointment = {
        id: info.event.extendedProps.appointmentId,
        date: info.event.start.toISOString().split('T')[0],
        time: info.event.start.toTimeString().slice(0, 5),
        notes: info.event.extendedProps.notes === "None" ? "" : info.event.extendedProps.notes,
        statusCode: info.event.extendedProps.statusCode
      };
      
      // Fill modal fields
      document.getElementById("sticky-title").textContent =
        info.event.title.split("\n")[0];
      document.getElementById("sticky-status").textContent =
        info.event.extendedProps.status;
      document.getElementById("sticky-date").textContent =
        info.event.start.toLocaleDateString();
      document.getElementById("sticky-time").textContent =
        info.event.start.toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }) +
        " - " +
        info.event.end.toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        });
      document.getElementById("sticky-notes").textContent =
        info.event.extendedProps.notes;

      // Show/hide buttons based on appointment status
      const updateButton = document.getElementById("update-appointment-btn");
      const cancelButton = document.getElementById("cancel-appointment-btn");
      const statusCode = info.event.extendedProps.statusCode;
      
      if (statusCode === 1 || statusCode === 2) { // Pending or Confirmed
        if (updateButton) {
          updateButton.style.display = "block";
          updateButton.onclick = () => openUpdateModal();
        }
        if (cancelButton) {
          cancelButton.style.display = "block";
          cancelButton.onclick = () => showCancelConfirmation();
        }
      } else {
        if (updateButton) updateButton.style.display = "none";
        if (cancelButton) cancelButton.style.display = "none";
      }

      // Show modal
      document.getElementById("sticky-note-modal").classList.remove("hidden");
    },
  });
  calendar.render();

  // After calendar.render();
  if (selectedId) {
    // Find the event by appointmentId and open its modal
    const event = calendar.getEvents().find(ev => ev.extendedProps && ev.extendedProps.appointmentId == selectedId);
    if (event) {
      // Simulate a click to open the modal
      calendar.trigger('eventClick', { event });
    }
  }

  document.getElementById("closeStickyNote").onclick = function () {
    document.getElementById("sticky-note-modal").classList.add("hidden");
  };
  document.getElementById("sticky-note-modal").onclick = function (e) {
    if (e.target === this) this.classList.add("hidden");
  };

  // Update modal event handlers
  document.getElementById("closeUpdateModal").onclick = function () {
    document.getElementById("update-appointment-modal").classList.add("hidden");
  };
  document.getElementById("cancel-update-btn").onclick = function () {
    document.getElementById("update-appointment-modal").classList.add("hidden");
  };
  document.getElementById("update-appointment-modal").onclick = function (e) {
    if (e.target === this) this.classList.add("hidden");
  };

  // Update form submission
  document.getElementById("update-appointment-form").onsubmit = function (e) {
    e.preventDefault();
    updateAppointment();
  };

  // Cancel confirmation modal event handlers
  document.getElementById("cancel-confirmation-no").onclick = function () {
    document.getElementById("cancel-confirmation-modal").classList.add("hidden");
  };
  
  document.getElementById("cancel-confirmation-yes").onclick = function () {
    document.getElementById("cancel-confirmation-modal").classList.add("hidden");
    performCancelAppointment();
  };

  // Close cancel confirmation modal when clicking outside
  document.getElementById("cancel-confirmation-modal").onclick = function (e) {
    if (e.target === this) this.classList.add("hidden");
  };
});

function renderStatusText(status) {
  switch (status) {
    case 1:
      return "Đang chờ";
    case 2:
      return "Đã xác nhận";
    case 3:
      return "Rescheduled";
    case 4:
      return "Rejected";
    case 5:
      return "Completed";
  }
}

function getStatusColor(status) {
  switch (status) {
    case 1:
      return "#fbc02d"; // Pending - yellow
    case 2:
      return "#43a047"; // Confirmed - green
    case 3:
      return "#e53935"; // Cancelled - red
    case 4:
      return "#e53935"; // Rejected - red
    case 5:
      return "#3498db"; // Completed - blue
  }
}

function addMinutes(time, minsToAdd) {
  // time: "09:00", minsToAdd: 30 => "09:30"
  const [h, m] = time.split(":").map(Number);
  const date = new Date(0, 0, 0, h, m + minsToAdd, 0, 0);
  return date.toTimeString().slice(0, 5);
}

// Open update modal function
function openUpdateModal() {
  const modal = document.getElementById("update-appointment-modal");
  
  // Pre-fill the form with current appointment data
  document.getElementById("update-date").value = window.currentAppointment.date;
  document.getElementById("update-time").value = window.currentAppointment.time;
  document.getElementById("update-notes").value = window.currentAppointment.notes;
  
  // Set minimum date to today
  const today = new Date().toISOString().split('T')[0];
  document.getElementById("update-date").min = today;
  
  // Close the sticky note modal
  document.getElementById("sticky-note-modal").classList.add("hidden");
  
  // Show update modal
  modal.classList.remove("hidden");
}

// Update appointment function
async function updateAppointment() {
  const token = localStorage.getItem("token");
  const appointmentId = window.currentAppointment.id;
  
  const formData = {
    appointmentDate: document.getElementById("update-date").value,
    appointmentTime: document.getElementById("update-time").value + ":00", // Add seconds
    notes: document.getElementById("update-notes").value || ""
  };

  try {
    // Show loading state
    const saveButton = document.getElementById("save-update-btn");
    saveButton.disabled = true;
    saveButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

    const response = await fetch(
      `https://localhost:7009/api/Appointment/UpdateAppointmentRequest?id=${appointmentId}`,
      {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData)
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to update appointment: ${response.status}`);
    }

    // Success - close modal and show success message
    document.getElementById("update-appointment-modal").classList.add("hidden");
    showNotification("Cập nhật lịch hẹn thành công! Vui lòng làm mới trang để xem thay đổi.", "success");
    
    // Optionally refresh the page after a delay
    setTimeout(() => {
      location.reload();
    }, 2000);

  } catch (error) {
    console.error("Error updating appointment:", error);
    showNotification("Không thể cập nhật lịch hẹn. Vui lòng thử lại.", "error");
    
    // Reset button state
    const saveButton = document.getElementById("save-update-btn");
    saveButton.disabled = false;
    saveButton.innerHTML = '<i class="fas fa-save"></i> Lưu thay đổi';
  }
}

// Show cancel confirmation modal
function showCancelConfirmation() {
  // Close the appointment details modal
  document.getElementById("sticky-note-modal").classList.add("hidden");
  
  // Show the cancel confirmation modal
  document.getElementById("cancel-confirmation-modal").classList.remove("hidden");
}

// Cancel appointment function
async function performCancelAppointment() {
  const token = localStorage.getItem("token");
  const appointmentId = window.currentAppointment.id;
  
  try {
    // Show loading state on the confirmation button
    const confirmButton = document.getElementById("cancel-confirmation-yes");
    confirmButton.disabled = true;
    confirmButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang hủy...';

    const response = await fetch(
      `https://localhost:7009/api/Appointment/ChangeAppointmentStatus?id=${appointmentId}&status=4`,
      {
        method: "PATCH",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to cancel appointment: ${response.status}`);
    }

    // Success - show success message and reload page
    showNotification("Đã hủy lịch hẹn thành công!", "success");
    
    // Reload the page to reflect changes
    setTimeout(() => {
      location.reload();
    }, 1500);

  } catch (error) {
    console.error("Error cancelling appointment:", error);
    showNotification("Không thể hủy lịch hẹn. Vui lòng thử lại.", "error");
    
    // Reset button state
    const confirmButton = document.getElementById("cancel-confirmation-yes");
    confirmButton.disabled = false;
    confirmButton.innerHTML = '<i class="fas fa-check"></i> Có, hủy lịch';
  }
}

// Cancel appointment function (deprecated - keeping for backward compatibility)
async function cancelAppointment(appointmentId, eventObj) {
  // This function is deprecated - redirecting to new confirmation modal
  showCancelConfirmation();
}

// Show notification function
function showNotification(message, type = "info") {
  const notification = document.createElement("div");
  notification.className = `notification ${type}`;
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 15px 20px;
    border-radius: 5px;
    color: white;
    font-weight: 500;
    z-index: 10000;
    max-width: 300px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    transition: all 0.3s ease;
  `;
  
  // Set background color based on type
  switch (type) {
    case "success":
      notification.style.backgroundColor = "#4caf50";
      break;
    case "error":
      notification.style.backgroundColor = "#f44336";
      break;
    default:
      notification.style.backgroundColor = "#2196f3";
  }
  
  notification.textContent = message;
  document.body.appendChild(notification);
  
  // Remove notification after 3 seconds
  setTimeout(() => {
    notification.style.opacity = "0";
    notification.style.transform = "translateX(100%)";
    setTimeout(() => {
      if (notification.parentNode) {
        notification.parentNode.removeChild(notification);
      }
    }, 300);
  }, 3000);
}
