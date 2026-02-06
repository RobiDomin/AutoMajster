document.addEventListener("DOMContentLoaded", function () {

    var dateElement = document.getElementById("datepicker");
    var submitBtn = document.querySelector("input[type='submit']");
    var msgSpan = document.getElementById("availability-message");
    if (!msgSpan && dateElement) {
        msgSpan = document.createElement("span");
        msgSpan.id = "availability-message";
        msgSpan.style.fontWeight = "bold";
        msgSpan.style.marginLeft = "10px";
        dateElement.parentNode.appendChild(msgSpan);
    }

    if (dateElement) {
        flatpickr("#datepicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i",
            time_24hr: true,
            locale: "pl",
            minDate: "today",
            defaultHour: 8,
            minuteIncrement: 30,

            onChange: function (selectedDates, dateStr, instance) {

                if (!dateStr) {
                    msgSpan.innerText = "";
                    submitBtn.disabled = false;
                    return;
                }

                var url = '/Reservations/CheckAvailability?date=' + encodeURIComponent(dateStr);

                fetch(url)
                    .then(response => response.json())
                    .then(data => {
                        if (data.isTaken) {
                            msgSpan.innerText = "❌ Termin zajęty!";
                            msgSpan.className = "text-danger";
                            submitBtn.disabled = true;
                        } else {
                            msgSpan.innerText = "✔️ Termin wolny!";
                            msgSpan.className = "text-success";
                            submitBtn.disabled = false;
                        }
                    })
                    .catch(error => console.error('Błąd AJAX:', error));
            }
        });

        console.log("Kalendarz z AJAXem załadowany.");
    }
});