import {publicAI} from './public-ai.mjs';
export default {
  async fetch(request,env) {
    const path=new URL(request.url).pathname;
    if(path.startsWith('/api/')) return publicAI(request,env);
    return env.ASSETS.fetch(request);
  }
};
