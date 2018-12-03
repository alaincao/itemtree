
import * as common from "./Views/common";

export const { e:Languages, a:allLanguages } = common.utils.strEnum([ 'en', 'fr', 'nl' ]);
export type Language = keyof typeof Languages;
