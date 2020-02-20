
import * as common from "../common";
import * as dtos from "../../DTOs/BlogPost";
import * as apiCtrl from "../../Services/BlogController";
import * as htmlCtrl from "./BlogController";

const blogIdAttribute = 'ttt-blog-id';

export async function init(p:{	$postsContainer	: JQuery,
								$endPostElement	: JQuery,
								$spinner		: JQuery,
								postsIncrement	: number,
							}) : Promise<void>
{
	common.utils.log( 'list.init() START', { p } );

	// Retreive images of initially loaded posts
	const lastPost = await loadImages();

	common.utils.log( 'list.init(): Wait for document.load' );
	await new Promise<void>( (resolve)=>$(window).on( 'load', ()=>resolve() ) );
	common.utils.log( 'list.init(): Loaded!' );

	// Start "scroll-triggered" retreives loop
	/*await*/ loopLoadNextPosts({ $endPostElement:p.$endPostElement, $spinner:p.$spinner, lastPost, postsIncrement:p.postsIncrement });

	common.utils.log( 'list.init() END' );
}

async function loopLoadNextPosts(p:{ $endPostElement:JQuery, $spinner:JQuery, lastPost:PostItem, postsIncrement:number }) : Promise<void>
{
	common.utils.log( 'loopLoadNextPosts(): START', { p } );

	let lastPost = p.lastPost;
	while( lastPost != null )
	{
		common.utils.log( 'loopLoadNextPosts(): Wait for $endPostElement to be visible' );
		await common.html.waitForScrolledVisible( p.$endPostElement );

		common.utils.log( `loopLoadNextPosts(): Request next '${p.postsIncrement}' posts` );
		p.$spinner.show();
		const html = await htmlCtrl.list.list({ noLayout:true, includeImages:false, skipToID:lastPost.id, take:p.postsIncrement });
		p.$spinner.hide();

		common.utils.log( 'loopLoadNextPosts(): Append received posts' );
		p.$endPostElement.before( html );

		common.utils.log( `loopLoadNextPosts(): Launch 'loadImages()'`, { lastPost } );
		lastPost = await loadImages();

		common.utils.log( 'loopLoadNextPosts()', { lastPost } );
	}

	common.utils.log( 'loopLoadNextPosts(): END' );
}

async function loadImages() : Promise<PostItem>
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

	common.utils.log( 'loadImages(): Asynchroneously retreive posts (full) DTOs', { posts } );
	posts.forEach( async (item)=>
		{
			common.html.block( item.$container );
			const rc = await apiCtrl.list({ id:item.id, includeInactives:true });
			common.html.unblock( item.$container );
			if(! rc.success )
			{
				common.utils.error( 'retreive post error', { rc } );
				// common.html.showMessage( rc.errorMessage ); <== publicly-accessible page => Don't show internal errors ...
				return;
			}
			item.model = rc.posts[0];

			common.utils.log( 'loadImages(): Bind model', { /*model:item.model,*/ $element:item.$container, id:item.id } );
			ko.applyBindings( item.model, item.$container[0] );
		} );

	const lastPostItem = (posts.length > 0) ? posts[posts.length - 1] : null;
	common.utils.log( 'loadImages(): END', { lastPostItem } );
	return lastPostItem;
}

interface PostItem
{
	id			: number;
	$container	: JQuery;
	model		: dtos.BlogPost;
}
