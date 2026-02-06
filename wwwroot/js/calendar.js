document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');

    if (calendarEl) {
        var calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            locale: 'pl',
            firstDay: 1,
            themeSystem: 'standard',

            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,listMonth'
            },

            buttonText: {
                today: 'Dziś',
                month: 'Miesiąc',
                week: 'Tydzień',
                list: 'Lista'
            },

            events: '/Reservations/GetEvents',

            height: 700,
            eventTimeFormat: {
                hour: '2-digit',
                minute: '2-digit',
                meridiem: false
            },

            eventFailure: function (errorObj) {
                console.error('Błąd pobierania rezerwacji:', errorObj);
            }
        });

        calendar.render();

        setTimeout(function () {
            calendar.updateSize();
        }, 200);
    }
});