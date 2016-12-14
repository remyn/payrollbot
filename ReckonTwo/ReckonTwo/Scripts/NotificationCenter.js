$(document).ready(function () {

   $("#notification-popover").popover({
        placement: "bottom",
        html : true,
        content: function() {
            var content = $(this).attr("data-popover-content");
            return $(content).children(".popover-body").html();
        },
        title: function() {
            var title = $(this).attr("data-popover-content");
            return $(title).children(".popover-heading").html();
        }
    });

   $(document).on("click", ".star", function () {
        $(this).toggleClass("star-checked");
   });

   $(document).on("click", ".selected", function () {
       $(this).removeClass("selected");
       var currentVal = $("#notificationBadge").text();
       $("#notificationBadge").text(currentVal-1);
       var data = parseInt($(this).data("key"));
       $.ajax({
           url: "/Notification/MarkAsRead",
           type: "POST",
           data: JSON.stringify ({ notificationId: data }),
           contentType: "application/json; charset=utf-8"
       });
   });

   $(document).on("click", ".ckbox label", function () {
        $(this).parents("tr").find("input").toggleClass("checked");
        $(this).parents("tr").toggleClass("selected");
    });

   $(document).on("click", ".btn-filter", function () {
        var $target = $(this).data("target");
        if ($target != "all") {
            $(".table tr").css("display", "none");
            $(".table tr[data-status=\"" + $target + "\"]").fadeIn("slow");
        } else {
            $(".table tr").css("display", "none").fadeIn("slow");
        }
    });

});