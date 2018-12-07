
export interface Result
{
	success			: boolean;
	errorMessage?	: string;
	log?			: string[];
	result?			: any;
}

export default Result;
