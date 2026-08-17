using System.Text;

namespace GummyDynasty.Simulation
{
    /// <summary>Tiny HTML tactical surfaces. Served by PhoneHost. Not a Unity UI.</summary>
    public static class PhonePages
    {
        public static string Join(string url)
        {
            return Page("Join", @"
<h1>GUMMY WARFARE</h1>
<p class='sub'>same LAN · host is the pit</p>
<button onclick=""go('commander')"">COMMANDER</button>
<button class='alt' onclick=""go('artillery')"">ARTILLERY</button>
<p id='err'></p>
<script>
async function go(role){
  const r = await fetch('/join',{method:'POST',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({op:'join',role:role})});
  const j = await r.json();
  if(!j.ok){ document.getElementById('err').textContent = j.error || 'no'; return; }
  location.href = '/' + role + '?p=' + encodeURIComponent(j.player);
}
</script>");
        }

        public static string Commander()
        {
            return Page("Commander", @"
<h1>COMMANDER</h1>
<p class='sub' id='meta'>hold WEST</p>
<canvas id='map' width='320' height='220'></canvas>
<div class='row'>
  <button onclick=""cmd('west')"">WEST</button>
  <button class='alt' onclick=""cmd('hold')"">HOLD</button>
</div>
<p id='err'></p>
<script>
const P = new URLSearchParams(location.search).get('p');
const c = document.getElementById('map').getContext('2d');
async function cmd(action){
  const r = await fetch('/cmd',{method:'POST',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({op:'cmd',role:'commander',player:P,action:action})});
  const j = await r.json();
  document.getElementById('err').textContent = j.ok ? '' : (j.error||'no');
}
function draw(s){
  c.fillStyle='#1b2430'; c.fillRect(0,0,320,220);
  function px(x,z){ return {x: 160 + x*8, y: 110 - z*8}; }
  c.strokeStyle='#3d5a4a';
  for(let i=-12;i<=12;i++){
    const a=px(i,-12), b=px(i,12); c.beginPath(); c.moveTo(a.x,a.y); c.lineTo(b.x,b.y); c.stroke();
    const c1=px(-12,i), c2=px(12,i); c.beginPath(); c.moveTo(c1.x,c1.y); c.lineTo(c2.x,c2.y); c.stroke();
  }
  if(s.wall){ const w=px(s.wall.x,s.wall.z); c.fillStyle='#c4a05a'; c.fillRect(w.x-6,w.y-18,12,36); }
  if(s.breach){ const b=px(s.breach.x,s.breach.z); c.fillStyle='#ffd24a'; c.beginPath(); c.arc(b.x,b.y,5,0,6.3); c.fill(); }
  if(s.flag){ const f=px(s.flag.x,s.flag.z); c.fillStyle='#e03040'; c.fillRect(f.x-3,f.y-14,6,18); }
  if(s.com){ const m=px(s.com.x,s.com.z); c.fillStyle='#ff7a18'; c.beginPath(); c.arc(m.x,m.y,7,0,6.3); c.fill(); }
  document.getElementById('meta').textContent =
    (s.mode||'WEST') + ' · ' + (s.tactic||'idle') + (s.victory?' · HELD':'');
}
async function tick(){
  const r = await fetch('/state'); const s = await r.json(); draw(s);
}
setInterval(tick, 400); tick();
</script>");
        }

        public static string Artillery()
        {
            return Page("Artillery", @"
<h1>ARTILLERY</h1>
<p class='sub' id='meta'>tap the map, then FIRE</p>
<canvas id='map' width='320' height='220'></canvas>
<div class='row'>
  <button onclick=""pick('catapult')"" id='bcat'>CATAPULT</button>
  <button class='alt' onclick=""pick('cannon')"" id='bcan'>CANNON</button>
</div>
<button onclick=""fire()"">FIRE</button>
<p id='err'></p>
<script>
const P = new URLSearchParams(location.search).get('p');
let machine = 'catapult';
let aim = {x:-2,z:0};
const c = document.getElementById('map').getContext('2d');
function pick(m){ machine=m; }
function world(ev){
  const r = ev.target.getBoundingClientRect();
  const x = (ev.clientX-r.left)/r.width*320;
  const y = (ev.clientY-r.top)/r.height*220;
  return {x:(x-160)/8, z:(110-y)/8};
}
document.getElementById('map').addEventListener('click', async ev=>{
  aim = world(ev);
  await fetch('/cmd',{method:'POST',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({op:'cmd',role:'artillery',player:P,action:'aim',machine:machine,x:aim.x,z:aim.z})});
});
async function fire(){
  const r = await fetch('/cmd',{method:'POST',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({op:'cmd',role:'artillery',player:P,action:'fire',machine:machine,x:aim.x,z:aim.z})});
  const j = await r.json();
  document.getElementById('err').textContent = j.ok ? '' : (j.error||'no');
}
function draw(s){
  c.fillStyle='#1b2430'; c.fillRect(0,0,320,220);
  function px(x,z){ return {x: 160 + x*8, y: 110 - z*8}; }
  if(s.wall){ const w=px(s.wall.x,s.wall.z); c.fillStyle='#c4a05a'; c.fillRect(w.x-6,w.y-18,12,36); }
  if(s.flag){ const f=px(s.flag.x,s.flag.z); c.fillStyle='#e03040'; c.fillRect(f.x-3,f.y-14,6,18); }
  if(s.com){ const m=px(s.com.x,s.com.z); c.fillStyle='#ff7a18'; c.beginPath(); c.arc(m.x,m.y,7,0,6.3); c.fill(); }
  const a=px(aim.x,aim.z); c.strokeStyle='#fff36a'; c.beginPath(); c.arc(a.x,a.y,8,0,6.3); c.stroke();
  document.getElementById('meta').textContent = machine + ' · ' + (s.tactic||'');
}
async function tick(){ const r = await fetch('/state'); draw(await r.json()); }
setInterval(tick, 400); tick();
</script>");
        }

        public static string JsonOk(string extra = null)
        {
            return extra == null ? "{\"ok\":true}" : "{\"ok\":true," + extra + "}";
        }

        public static string JsonErr(string error)
        {
            return "{\"ok\":false,\"error\":\"" + Esc(error) + "\"}";
        }

        public static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        static string Page(string title, string body)
        {
            var sb = new StringBuilder(2048);
            sb.Append("<!doctype html><html><head><meta charset='utf-8'>");
            sb.Append("<meta name='viewport' content='width=device-width,initial-scale=1,maximum-scale=1'>");
            sb.Append("<title>").Append(title).Append("</title><style>");
            sb.Append("body{margin:0;font-family:sans-serif;background:#241018;color:#fff;text-align:center;padding:16px}");
            sb.Append("h1{margin:8px 0 4px;font-size:28px;letter-spacing:.04em}");
            sb.Append(".sub{opacity:.8;margin:0 0 16px}");
            sb.Append("button{width:90%;max-width:360px;margin:8px 0;padding:18px;font-size:22px;font-weight:700;");
            sb.Append("border:0;border-radius:14px;background:#ff4b7a;color:#fff}");
            sb.Append("button.alt{background:#ff9a1f}");
            sb.Append(".row button{width:46%;margin:8px 2%;display:inline-block}");
            sb.Append("canvas{background:#1b2430;border-radius:12px;width:92%;max-width:360px}");
            sb.Append("#err{color:#ffd36a;min-height:1.2em}");
            sb.Append("</style></head><body>");
            sb.Append(body);
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
