$(function () {
    var hash = window.location.hash;
    if (hash) {
        $(hash.replace(".", "\\.")).show();
        $(hash.replace(".", "\\.")).parents().show();
    } else $("#user-manual").show();

    
    $('a').click(function() {
        $("body > div:not(#TOC)").hide();
        $($(this).attr("href").replace(".", "\\.")).show();
        $($(this).attr("href").replace(".", "\\.")).parents().show();
    });

    $("#TOC").resizable({
        handles: "e",
        resize: function(event, ui) {
            $("body > div:not(#TOC)").css("padding-left", ui.size.width);
        }
    });
    $("#TOC").scroll(function() {
        $(".ui-resizable-handle").css('top', $("#TOC").scrollTop());
    });
});