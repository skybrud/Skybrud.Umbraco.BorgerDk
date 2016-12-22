function changeSelected() {
    document.getElementById('body_selectedfields').value='';
    var fields = document.getElementById('body_fields').value.split(';');
    for(i=1; i<fields.length; i++)
    {
        if(document.getElementById('body_'+fields[i]).checked)
        {
            document.getElementById('body_selectedfields').value+=';'+fields[i];
        }
    }
}




if (typeof (Array.prototype.remove) == 'undefined') {
    Array.prototype.remove = function (element) {
        for (var i = 0; i < this.length; i++) {
            if (this[i] == element)
                this.splice(i, 1);
        }
    };
}

if (typeof (Array.prototype.contains) == 'undefined') {
    Array.prototype.contains = function(element) {
        for (var i = 0; i < this.length; i++) {
            if (this[i] == element) return true;
        }
        return false;
    };
}





$(document).ready(function() {

	/*$('a.borgerDkItemTrigger').click(function() {
        _mother = $(this).parent("li");
        $('.borgerDkItemContent',_mother).toggleSlide(500);
        return false;
    });*/

	$('.borgerDkPanel').each(function(i, e){

		var panel = $(e);

		var propertyId = panel.data('propertyid');
		var mandatory = panel.data('mandatory').split(',');
		var allowedTypes = panel.data('types').split(',');
		var municipalityId = panel.data('municipalityid');

		var input = $('.borgerDkUrl', panel);
		var summary = $('.borgerDkSummary', panel);
		var loader = $('.borgerDkLoader', panel);
		var content = $('.borgerDkContent', panel);
		var errors = $('.borgerDkErrors', panel);
		var example = $('.borgerDkExample', panel);

		/*var dev = $('<div style="background: #efefef; font-size: 10px;" />').prependTo(panel);

		var examples = [
			'https://www.borger.dk/Sider/boern-med-handicap.aspx',
			'https://www.borger.dk/Sider/folkepension.aspx',
			'https://www.borger.dk/Sider/Boligstoette-til-foertidspensionister.aspx',
			'https://lifeindenmark.borger.dk/Pages/Facts-about-Denmark.aspx'
		];

		$.each(examples, function(ii,ee) {
			$('<a href="#" style="display: block; line-height: 12px;">' + ee + '</a>').appendTo(dev).click(function(){
				input.val(ee);
				input.change();
				return false;
			});
		});*/

		var selectedUrl = input.val().split('?')[0];

		var selected = $('.borgerDkSelected', panel).val();
		selected = selected ? selected.split(',') : [];
	
		function isValidUrl(url) {
			return (url ? url + '' : '').match(/^https:\/\/([a-z]+).borger.dk\//) != null;
		}

		function toggle(trigger, subject) {
			if (subject.is(':visible')) {
				trigger.text('Vis dette indhold');
				trigger.closest('li').removeClass('expanded');
				subject.slideUp(500);
			} else {
				trigger.text('Skjul dette indhold');
				trigger.closest('li').addClass('expanded');
				subject.slideDown(500);
			}
		}

		var counter = 0;

		function getArticle(url, cache) {

			cache = (cache === false ? false : true);

			loader.show();
			content.html('');
			summary.hide();
			errors.hide();
			example.hide();

			$.getJSON('/base/BorgerDk/GetArticle.aspx', { url: url, cache: cache, municipalityId: municipalityId }, function(json) {
				getArticleCallback(json, selectedUrl, url);
			});
		
		}

		function getArticleCallback(json, lastUrl, newUrl) {

			if (json.meta.code == 200) {

				$('.borgerDkArticleId', summary).text(json.data.id);
				$('.borgerDkArticlePublished', summary).text(json.data.published);
				$('.borgerDkArticleUpdated', summary).text(json.data.modified);

				var count = 0;
			
				var list = $('<ul class="borgerDkContentList"/>').appendTo(content);

				$('<li class="borgerDkListHeader"><span>Vælg</span><span>Beskrivelse</span></li>').appendTo(list);
				
				$.each(json.data.elements, function(i,e) {

					if (!mandatory.contains(e.type) && !allowedTypes.contains(e.type)) return;
				
					if (e.type == 'title') {
						
						var li = $('<li/>').appendTo(list);
						var ch = $('<input id="' + e.id + '_Data' + propertyId + '" type="checkbox" />').attr('checked', selected.contains(e.id)).appendTo(li);
						var lb = $('<label for="' + e.id + '_Data' + propertyId + '">' + e.content + ' <span class="borgerDkItemClass">(overskrift)</span></label>').appendTo(li);

						// Check and disable if mandatory
						if (mandatory.contains(e.type)) ch.attr('checked', true).attr('disabled', true);
					
					} else if (e.type == 'header') {

						var li = $('<li/>').appendTo(list);
						var ch = $('<input id="' + e.id + '_Data' + propertyId + '" type="checkbox" />').attr('checked', selected.contains(e.id)).appendTo(li);
						var lb = $('<label for="' + e.id + '_Data' + propertyId + '">' + (e.content.length > 50 ? e.content.substring(0, 50) + '...' : e.content) + ' <span class="borgerDkItemClass">(manchet)</span></label>').appendTo(li);
						var ln = $('<a class="borgerDkItemTrigger" href="#">Vis dette indhold</a>').appendTo(li);

						var div = $('<div/>').addClass('borgerDkItemContent').html(e.content ? e.content : '').appendTo(li);

						ln.click(function(){
							toggle(ln, div);
							return false;
						});

						// Check and disable if mandatory
						if (mandatory.contains(e.type)) ch.attr('checked', true).attr('disabled', true);
					
					} else if (e.type == 'kernetekst') {
						
						var li = $('<li/>').appendTo(list);
						var ch = $('<input id="' + e.id + '_Data' + propertyId + '" type="checkbox" />').attr('checked', selected.contains(e.id)).appendTo(li);
						var lb = $('<label for="' + e.id + '_Data' + propertyId + '">' + e.text + '</label>').appendTo(li);

						// Update all nested checkboxes to reflect the change
						$('input:checkbox', li).change(function(){
							var state = $(this).is(':checked');
							$('ul input:checkbox', li).attr('checked', state);
						});

						// Check and disable if mandatory
						if (mandatory.contains(e.type)) ch.attr('checked', true).attr('disabled', true);

						var ul = $('<ul class="borgerDkSubContentList"/>').appendTo(li);

						$.each(e.content, function(j,m){
						
							var micro = $('<li/>').appendTo(ul);
							var microCh = $('<input id="microArticle-' + m.id + '_Data' + propertyId + '" type="checkbox" />').attr('checked', selected.contains(e.id) || selected.contains('microArticle-' + m.id)).appendTo(micro);
							$('<label for="microArticle-' + m.id + '_Data' + propertyId + '">' + m.text + ' <span class="borgerDkItemClass">(mikroartikel)</span></label>').appendTo(micro);

							$('label', micro).append('<a class="borgerDkItemTrigger" href="#">Vis dette indhold</a>');

							var div = $('<div/>').addClass('borgerDkItemContent').html(m.content).appendTo(micro);

							$('label a', micro).click(function(){
								toggle($(this), div);
								return false;
							});
							
							// Check and disable if mandatory
							if (mandatory.contains(e.type)) microCh.attr('checked', true).attr('disabled', true);
						
							// Update the parent checkbox to reflect the change
							$('input:checkbox', micro).change(function(){
								var checked = true;
								$('ul input:checkbox', li).each(function(k,c){
									if (!$(c).is(':checked')) checked = false;
								});
								if (!checked) $('>input:checkbox', li).attr('checked', false);
							});

						});
					
					} else {
						
						var li = $('<li/>').appendTo(list);
						var ch = $('<input id="' + e.id + '_Data' + propertyId + '" type="checkbox" />').attr('checked', selected.contains(e.id)).appendTo(li);
						var lb = $('<label for="' + e.id + '_Data' + propertyId + '">' + e.text + ' <span class="borgerDkItemClass">(' + e.id + ')</span></label>').appendTo(li);

						if (e.id != 'byline...') {

							var ln = $('<a class="borgerDkItemTrigger" href="#">Vis dette indhold</a>').appendTo(li);
							
							var div = $('<div/>').addClass('borgerDkItemContent').html(e.content ? e.content : '').appendTo(li);

							ln.click(function(){
								toggle(ln, div);
								return false;
							});

						}
						
						// Check and disable if mandatory
						if (mandatory.contains(e.id)) ch.attr('checked', true).attr('disabled', true);
					
					}

					count++;
				
				});

				if (count == 0) {
						
					var li = $('<li/>').css('padding-left', '10px').text('Den angivne artikel har ingen tilgængelige indholdstyper.').appendTo(list);

				}

				$('input:checkbox', panel).change(function(){

					var selected = [];
				
					$('input:checkbox', panel).each(function(i,e){
						if (e.checked) selected.push(e.id.split('_')[0]);
					});

					$('.borgerDkSelected', panel).val(selected.join(','));

					//console.log(selected);
				
				});

				loader.hide();
				summary.show();

			} else {

			    //errors.html('Der skete en fejl i forbindelse med forespørgslen til Borger.dk').show();
			    errors.html(json.meta.error).show();

				loader.hide();
			
			}
		
		}

		function resetArticle(panel) {
			example.show();
			loader.hide();
			summary.hide();
			content.html('');
			input.val('');
			selectedUrl = '';
		}

		function onChange() {
			var url = input.val().split('?')[0];
			if (url != input.val()) {
				input.val(url);
				return;
			}
			if (url == selectedUrl) return;
			if (isValidUrl(url)) getArticle(url, true);

			
			example.show();
			loader.show();
			summary.hide();
			content.html('');
			selectedUrl = url;
		}
			
		input.change(onChange);
		input.keyup(onChange);
	
		if (isValidUrl(selectedUrl)) {
			getArticle(selectedUrl);
		}
	
	});

	// https://www.borger.dk/Sider/boern-med-handicap.aspx
	// https://www.borger.dk/Sider/folkepension.aspx
	// https://www.borger.dk/Sider/Boligstoette-til-foertidspensionister.aspx
	// https://lifeindenmark.borger.dk/Pages/Facts-about-Denmark.aspx

// 'https://www.borger.dk/Sider/Boligstoette-til-foertidspensionister.aspx'.match(/https:\/\/www.borger.dk\/.+?\.aspx/)

});