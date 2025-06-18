// SignalR connection for real-time event updates
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/eventHub')
  .withAutomaticReconnect()
  .build();

// Start the connection
connection
  .start()
  .then(function () {
    console.log('SignalR Connected');
  })
  .catch(function (err) {
    console.error('SignalR Connection Error: ', err.toString());
  });

// Join event group when viewing event details
function joinEventGroup(eventId) {
  if (connection.state === signalR.HubConnectionState.Connected) {
    connection
      .invoke('JoinEventGroup', eventId.toString())
      .then(() => console.log(`Joined Event_${eventId} group`))
      .catch((err) => console.error('Error joining group:', err));
  } else {
    console.warn('SignalR not connected, retrying...');
    setTimeout(() => joinEventGroup(eventId), 1000);
  }
}

// Leave event group
function leaveEventGroup(eventId) {
  if (connection.state === signalR.HubConnectionState.Connected) {
    connection
      .invoke('LeaveEventGroup', eventId.toString())
      .then(() => console.log(`Left Event_${eventId} group`))
      .catch((err) => console.error('Error leaving group:', err));
  }
}

// Listen for event updates
connection.on('EventUpdated', function (message) {
  showNotification('Event Updated', message, 'info');
  // Refresh event details if on event page
  if (window.location.pathname.includes('/Events/Details/')) {
    setTimeout(() => location.reload(), 2000);
  }
});

// Listen for new attendee registrations
connection.on('AttendeeRegistered', function (attendeeName) {
  showNotification(
    'New Registration',
    `${attendeeName} just registered for this event!`,
    'success'
  );
  // Update attendee count
  updateAttendeeCount();

  // Refresh relevant pages
  if (
    window.location.pathname.includes('/Events/Details/') ||
    window.location.pathname.includes('/Attendees/EventAttendees/')
  ) {
    setTimeout(() => location.reload(), 2000);
  }
});

// Listen for attendee unregistrations
connection.on('AttendeeUnregistered', function (attendeeName) {
  showNotification(
    'Unregistered',
    `${attendeeName} unregistered from this event.`,
    'warning'
  );

  // Refresh relevant pages
  if (
    window.location.pathname.includes('/Events/Details/') ||
    window.location.pathname.includes('/Attendees/EventAttendees/')
  ) {
    setTimeout(() => location.reload(), 2000);
  }
});

// Listen for new events created
connection.on('EventCreated', function (eventTitle) {
  showNotification('New Event', `"${eventTitle}" has been created!`, 'primary');
  // Refresh events list if on events page
  if (window.location.pathname.includes('/Events')) {
    setTimeout(() => location.reload(), 2000);
  }
});

// Listen for events deleted
connection.on('EventDeleted', function (eventTitle) {
  showNotification(
    'Event Deleted',
    `"${eventTitle}" has been deleted.`,
    'warning'
  );
  // Refresh events list if on events page
  if (window.location.pathname.includes('/Events')) {
    setTimeout(() => location.reload(), 2000);
  }
});

// Show notification function with better styling
function showNotification(title, message, type = 'info') {
  const alertClass = `alert-${type}`;
  const iconClass = getIconClass(type);

  const notification = `
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 80px; right: 20px; z-index: 1050; min-width: 350px; box-shadow: 0 4px 8px rgba(0,0,0,0.3);" 
             role="alert">
            <i class="${iconClass} me-2"></i>
            <strong>${title}:</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

  // Remove existing notifications of same type to avoid clutter
  $(`.alert-${type}`).fadeOut(300, function () {
    $(this).remove();
  });

  $('body').append(notification);

  // Auto-remove after 5 seconds
  setTimeout(function () {
    $(`.alert-${type}`).fadeOut(300, function () {
      $(this).remove();
    });
  }, 5000);
}

// Get appropriate icon for notification type
function getIconClass(type) {
  switch (type) {
    case 'success':
      return 'fas fa-check-circle';
    case 'warning':
      return 'fas fa-exclamation-triangle';
    case 'danger':
      return 'fas fa-times-circle';
    case 'info':
      return 'fas fa-info-circle';
    case 'primary':
      return 'fas fa-bell';
    default:
      return 'fas fa-info-circle';
  }
}

// Update attendee count
function updateAttendeeCount() {
  const countElement = document.getElementById('attendee-count');
  if (countElement) {
    const currentCount = parseInt(countElement.textContent);
    countElement.textContent = currentCount + 1;
  }

  // Also update badge counts
  const badgeElements = document.querySelectorAll('.badge');
  badgeElements.forEach((badge) => {
    if (
      badge.textContent.includes('Attendees') ||
      badge.previousElementSibling?.textContent?.includes('Attendees')
    ) {
      const currentCount = parseInt(badge.textContent);
      if (!isNaN(currentCount)) {
        badge.textContent = currentCount + 1;
      }
    }
  });
}

// Auto-reconnect handling
connection.onreconnecting(() => {
  console.log('SignalR reconnecting...');
  showNotification('Connection', 'Reconnecting to server...', 'warning');
});

connection.onreconnected(() => {
  console.log('SignalR reconnected');
  showNotification('Connection', 'Reconnected to server!', 'success');

  // Rejoin current event group if on event details page
  const eventId = getEventIdFromUrl();
  if (eventId) {
    joinEventGroup(eventId);
  }
});

connection.onclose(() => {
  console.log('SignalR connection closed');
  showNotification(
    'Connection',
    'Connection lost. Please refresh the page.',
    'danger'
  );
});

// Helper function to get event ID from URL
function getEventIdFromUrl() {
  const path = window.location.pathname;
  const match = path.match(/\/Events\/Details\/(\d+)/);
  return match ? match[1] : null;
}

// Export functions for use in other scripts
window.EventSignalR = {
  joinEventGroup,
  leaveEventGroup,
  showNotification,
  connection,
};
