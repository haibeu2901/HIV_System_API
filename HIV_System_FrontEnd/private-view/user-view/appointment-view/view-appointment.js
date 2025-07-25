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

    events = data.map((appt) => {
      // For pending appointments (status 1), use requestDate and requestTime
      // For other statuses, use apmtDate and apmTime
      const displayDate = appt.apmStatus === 1 ? appt.requestDate : appt.apmtDate;
      const displayTime = appt.apmStatus === 1 ? appt.requestTime : appt.apmTime;
      
      // Skip appointments that don't have valid date/time
      if (!displayDate || !displayTime) {
        return null;
      }
      
      return {
        title: `Dr. ${appt.doctorName}\n${displayTime.slice(0, 5)} (${renderStatusText(
          appt.apmStatus
        )})`,
        start: `${displayDate}T${displayTime}`,
        end: `${displayDate}T${addMinutes(displayTime.slice(0, 5), 30)}`, // 30 min duration
        color: getStatusColor(appt.apmStatus),
        extendedProps: {
          appointmentId: appt.appointmentId,
          notes: appt.notes || "None",
          status: renderStatusText(appt.apmStatus),
          statusCode: appt.apmStatus,
          originalDate: appt.apmtDate,
          originalTime: appt.apmTime,
          requestDate: appt.requestDate,
          requestTime: appt.requestTime,
        },
      };
    })
    .filter(event => event !== null && event.extendedProps.statusCode !== 3 && event.extendedProps.statusCode !== 4); // Remove cancelled appointments and null events
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
      const eventStatusCode = info.event.extendedProps.statusCode;
      
      // For pending appointments (status 1), use request date/time, otherwise use appointment date/time
      let displayDate, displayTime;
      if (eventStatusCode === 1) {
        displayDate = info.event.extendedProps.requestDate;
        displayTime = info.event.extendedProps.requestTime;
      } else {
        displayDate = info.event.extendedProps.originalDate || info.event.start.toISOString().split('T')[0];
        displayTime = info.event.extendedProps.originalTime || info.event.start.toTimeString().slice(0, 8);
      }
      
      window.currentAppointment = {
        id: info.event.extendedProps.appointmentId,
        date: displayDate,
        time: displayTime ? displayTime.slice(0, 5) : info.event.start.toTimeString().slice(0, 5),
        notes: info.event.extendedProps.notes === "None" ? "" : info.event.extendedProps.notes,
        statusCode: eventStatusCode
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
      
      if (eventStatusCode === 1 || eventStatusCode === 2) { // Pending or Confirmed
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
  // time: "09:00" or "09:00:00", minsToAdd: 30 => "09:30"
  const timeParts = time.split(":");
  const h = parseInt(timeParts[0]);
  const m = parseInt(timeParts[1]);
  const date = new Date(0, 0, 0, h, m + minsToAdd, 0, 0);
  return date.toTimeString().slice(0, 5);
}

// Open update modal function
function openUpdateModal() {
  const modal = document.getElementById("update-appointment-modal");
  
  // Pre-fill the form with current appointment data
  // For pending appointments (status 1), use request date/time, otherwise use appointment date/time
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
        method: "PUT",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData)
      }
    );

    if (!response.ok) {
      const errorData = await response.text();
      
      // Handle specific error cases
      if (response.status === 409) {
        // Conflict - usually doctor availability issues
        const errorMessage = errorData || 'Conflict occurred';
        
        // Check if it's a doctor availability error
        if (errorMessage.includes('lịch làm việc') || errorMessage.includes('working hours') || 
            errorMessage.includes('khả dụng') || errorMessage.includes('available')) {
          // Extract working hours from error message if possible
          const timePattern = /(\d{2}:\d{2}\s*-\s*\d{2}:\d{2})/g;
          const workingHours = errorMessage.match(timePattern);
          
          let userFriendlyMessage = 'Thời gian yêu cầu không nằm trong giờ làm việc của bác sĩ.';
          if (workingHours && workingHours.length > 0) {
            userFriendlyMessage += `\n\nGiờ làm việc của bác sĩ: ${workingHours.join(', ')}`;
            userFriendlyMessage += '\n\nVui lòng chọn thời gian khác trong khung giờ làm việc.';
          }
          
          showNotification(userFriendlyMessage, "error");
          
          // Reset button state and return early
          const saveButton = document.getElementById("save-update-btn");
          saveButton.disabled = false;
          saveButton.innerHTML = '<i class="fas fa-save"></i> Lưu thay đổi';
          return;
        }
      }
      
      // Handle other HTTP errors
      let userErrorMessage = 'Không thể cập nhật lịch hẹn. ';
      
      switch (response.status) {
        case 400:
          userErrorMessage += 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.';
          break;
        case 401:
          userErrorMessage += 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
          break;
        case 403:
          userErrorMessage += 'Bạn không có quyền thực hiện thao tác này.';
          break;
        case 404:
          userErrorMessage += 'Không tìm thấy lịch hẹn.';
          break;
        case 409:
          userErrorMessage += 'Có xung đột trong dữ liệu. Vui lòng thử lại.';
          break;
        case 500:
          userErrorMessage += 'Lỗi hệ thống. Vui lòng thử lại sau.';
          break;
        default:
          userErrorMessage += 'Vui lòng thử lại.';
      }
      
      throw new Error(userErrorMessage);
    }

    // Get the updated appointment data from response
    const updatedAppointment = await response.json();
    console.log('Updated appointment data:', updatedAppointment);

    // Success - close modal and show success message with updated info
    document.getElementById("update-appointment-modal").classList.add("hidden");
    
    // Show detailed success message based on appointment status
    if (updatedAppointment.apmStatus === 1) {
      // Pending appointment - show request date/time
      const requestDate = new Date(updatedAppointment.requestDate).toLocaleDateString('vi-VN');
      const requestTime = updatedAppointment.requestTime.slice(0, 5);
      showNotification(
        `Cập nhật yêu cầu lịch hẹn thành công!\nNgày yêu cầu: ${requestDate}\nGiờ yêu cầu: ${requestTime}\nTrạng thái: Đang chờ xác nhận`,
        "success"
      );
    } else {
      // Confirmed appointment - show appointment date/time
      const appointmentDate = updatedAppointment.apmtDate ? 
        new Date(updatedAppointment.apmtDate).toLocaleDateString('vi-VN') : 'Chưa xác định';
      const appointmentTime = updatedAppointment.apmTime ? 
        updatedAppointment.apmTime.slice(0, 5) : 'Chưa xác định';
      showNotification(
        `Cập nhật lịch hẹn thành công!\nNgày hẹn: ${appointmentDate}\nGiờ hẹn: ${appointmentTime}`,
        "success"
      );
    }
    
    // Reload the page to reflect changes
    setTimeout(() => {
      location.reload();
    }, 2500);

  } catch (error) {
    console.error("Error updating appointment:", error);
    
    // Show user-friendly error message
    const errorMessage = error.message || "Không thể cập nhật lịch hẹn. Vui lòng thử lại.";
    showNotification(errorMessage, "error");
    
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
    border-radius: 8px;
    color: white;
    font-weight: 500;
    z-index: 10000;
    max-width: 350px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    transition: all 0.3s ease;
    white-space: pre-line;
    line-height: 1.4;
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
  
  // Remove notification after longer time for error messages
  const displayTime = type === "error" ? 6000 : 3000;
  setTimeout(() => {
    notification.style.opacity = "0";
    notification.style.transform = "translateX(100%)";
    setTimeout(() => {
      if (notification.parentNode) {
        notification.parentNode.removeChild(notification);
      }
    }, 300);
  }, displayTime);
}
