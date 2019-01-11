
import * as common from "../common";
import * as dtos from "../../DTOs/BlogPost";
import * as ctrl from "../../Services/BlogController";

const blogIdAttribute = 'ttt-blog-id';

export async function init(p:{	$postsContainer	: JQuery,
								$endPostElement	: JQuery,
								$spinner		: JQuery,
							}) : Promise<void>
{
	common.utils.log( 'list.init() START', { p } );

	await loadImages();

	common.utils.log( 'list.init() END' );
}

async function loadImages() : Promise<void>
{
	common.utils.log( 'loadImages(): START' );

	// Find all new elements with the "tag attribute" (i.e. all new blog posts)
	const posts : PostItem[] = [];
	$(`[${blogIdAttribute}]`).each( (i,e)=>
		{
			const $container = $(e);
			const id = parseInt( $container.attr(blogIdAttribute) );
			$container.removeAttr( blogIdAttribute );  // Remove the "tag attribute" from the DOM so that next call to this method does not re-find this item
			posts.push({ id, $container, model:null });
		} );
	common.utils.log( 'loadImages()', { posts } );

	// Retreive the (full) models of these posts
	const tasks = posts.map( async (item)=>
		{
			common.html.block( item.$container );
			const rc = await ctrl.list({ id:item.id });
			common.html.unblock( item.$container );
			if(! rc.success )
			{
				common.utils.error( 'retreive post error', { rc } );
				common.html.showMessage( rc.errorMessage );
				return;
			}
			item.model = rc.posts[0];

			ko.applyBindings( item.model, item.$container[0] );
		} );

	common.utils.log( 'loadImages(): Wait for all tasks to terminate' );
	await Promise.all( tasks );

	common.utils.log( 'loadImages(): END' );
}

interface PostItem
{
	id			: number;
	$container	: JQuery;
	model		: dtos.BlogPost;
}
