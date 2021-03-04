
import * as tmce from "tinymce";
import * as common from "../common";

/** Add 'tinymceEditor' Knockout bindings handler */
var koTinymceEditorAdded = false;
export function addKoTinymceEditor() : void
{
	if( koTinymceEditorAdded == true )
		// Already done
		return;
	common.utils.log( 'addKoTinymceEditor()' );
	koTinymceEditorAdded = true;

	// Create HTML editor KO's binding
	ko.bindingHandlers.tinymceEditor =
		{
			init : function(element, valueAccessor, allBindings, viewModel, bindingContext)
				{
					const $element = $(element);
					const observable : KnockoutObservable<string> = valueAccessor();

					// Initial sync.
					$element.html( observable() );

					(<any>$element).tinymce( {
							inline	: true,
							plugins	: 'print preview importcss searchreplace autolink save directionality visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons code',
										// anavailable: powerpaste casechange tinydrive advcode mediaembed checklist tinymcespellchecker a11ychecker formatpainter permanentpen pageembed tinycomments mentions linkchecker advtable
										// Unwanted: autosave(reload confirmation dialog)
							setup	: function(ed:tmce.Editor)
										{
											ed.on( 'change', function()
												{
													// HTML has changed
													observable( $element.html() );
												} );
										},
						} );
				},
			update : function(element, valueAccessor)
				{
					// Observable has changed
					const observable : KnockoutObservable<string> = valueAccessor();
					const newHtml = observable();
					const $element = $(element);
					const origHtml = $element.html();
					if( newHtml != origHtml )
						// HTML has changed
						$element.html( newHtml );
					else
						{/*nb: Don't update HTML when not necessary or we loose the cursor position*/}
				},
		};
}
