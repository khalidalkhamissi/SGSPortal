/* ═══════════════════════════════════════════════════
   SGS Forms — Shared Navigation — nav.js
   نظام تعبئة النماذج أون لاين
   Inject into every page via <script src="nav.js">
═══════════════════════════════════════════════════ */
(function () {
  // الواجهة والـ API يُخدَمان من نفس الخادم
  const API  = window.location.origin;
  const S    = localStorage;

  // تهريب HTML — كل نص قادم من الخادم أو المستخدم يمر به قبل innerHTML
  const esc = v => String(v == null ? '' : v).replace(/[&<>"'`]/g, c =>
    ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', '"':'&quot;', "'":'&#39;', '`':'&#96;' }[c]));
  // لون CSS آمن فقط (#RRGGBB) — أي قيمة أخرى تُستبدل بالبديل
  const safeColor = (c, fb) => /^#[0-9A-Fa-f]{6}$/.test(c || '') ? c : fb;
  window.esc = esc;
  let lang   = S.getItem('lang') || 'en';

  // ── Auth check ──────────────────────────────────
  const token    = S.getItem('token');
  const role     = S.getItem('role')         || '';
  const uname    = S.getItem('name')         || 'User';
  const airName  = S.getItem('airport_name') || '';

  const curPage  = location.pathname.split('/').pop() || 'index.html';

  if (!token && curPage !== 'index.html') {
    location.href = 'index.html'; return;
  }

  // ── Permissions ─────────────────────────────────
  const PERMS = new Set((S.getItem('perms') || '').split(' ').filter(Boolean));
  const can   = p => PERMS.has(p);

  // رمز صادر قبل ترقية الأدوار لا يحمل صلاحيات: قائمة فارغة و403 صامت.
  // يُطرح ويُطلب دخول جديد بدل ترك المستخدم أمام شاشة بلا تفسير.
  if (token && PERMS.size === 0 && curPage !== 'index.html') {
    S.removeItem('token'); location.href = 'index.html'; return;
  }

  // ── Labels ──────────────────────────────────────
  const L = {
    ar: {
      app:'منظومة التقارير', sub:'SGS Forms',
      reports:'الرئيسية', coordination:'التنسيق', pending:'بانتظار الموافقة', drafts:'المسودات', returned:'التقارير المُعادة', approved:'التقارير المعتمدة',
      dashboard:'لوحة التحكم', users:'الإدارة', logs:'سجل التحديثات',
      logout:'خروج',
      roles:{ admin:'مدير النظام', management:'الإدارة', supervisor:'مشرف', data_entry:'مدخل بيانات' },
      sec_ops:'التشغيل', sec_view:'المراقبة', sec_admin:'النظام',
    },
    en: {
      app:'Reports System', sub:'SGS Forms',
      reports:'Home', coordination:'Coordination', pending:'Pending Approval', drafts:'Drafts', returned:'Returned Reports', approved:'Approved Reports',
      dashboard:'Dashboard', users:'Administration', logs:'Activity Logs',
      logout:'Logout',
      roles:{ admin:'Admin', management:'Management', supervisor:'Supervisor', data_entry:'Data Entry' },
      sec_ops:'Operations', sec_view:'Monitoring', sec_admin:'System',
    },
  };

  // اللون والاسم يصلان من القاعدة عند الدخول؛ الخريطة الثابتة بديل احتياطي فقط
  const ROLE_COLOR = { admin:'#7C3AED', management:'#C9A84C', supervisor:'#2563EB', data_entry:'#006C4E' };
  const roleColor  = S.getItem('role_color') || '';
  const roleNameAr = S.getItem('role_name_ar') || '';
  const roleNameEn = S.getItem('role_name_en') || '';

  // ── Build HTML ───────────────────────────────────
  function html(l) {
    const t   = L[l] || L.ar;
    const rc  = safeColor(roleColor, ROLE_COLOR[role] || '#006C4E');
    const rl  = esc((l === 'ar' ? roleNameAr : roleNameEn) || t.roles[role] || role);
    const av  = esc(uname.charAt(0).toUpperCase());
    const act = p => curPage === p ? 'sgs-active' : '';
    // التنسيق يشمل صفحتين: الهبوط والنموذج — act() تطابق صفحة واحدة فقط
    const coordAct = (curPage === 'coordination.html' || curPage === 'coordination_form.html') ? 'sgs-active' : '';
    const dir = l === 'ar' ? 'rtl' : 'ltr';

    // كل عنصر يظهر بصلاحيته — لا باسم الدور
    const showReports  = can('reports.view');
    const showCoord    = can('coordination.view');
    const showPending  = can('reports.pending');
    const showDrafts   = can('reports.drafts');
    const showReturned = can('reports.returned');
    const showApproved = can('reports.approved');
    const showDash     = can('dashboard.view');
    const showAdmin    = can('admin.users') || can('admin.roles') || can('admin.stations');
    const showLogs     = can('logs.view');
    const showAdminSec = showAdmin || showLogs;

    const airInfo = airName
      ? `<div class="sg-airport">${esc(airName)}</div>`
      : '';

    return `
<style>
@import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&family=DM+Mono:wght@400;500&display=swap');
:root{
  --g:#006C4E;--gm:#009966;--gl:#E8F5F0;
  --gold:#C9A84C;--goldl:#FDF8EE;
  --blue:#2563EB;--bluel:#EEF4FF;
  --red:#DC2626;--redl:#FEF2F2;
  --bg:#F4F6F9;--border:#E2E8F0;
  --text:#0F172A;--muted:#64748B;
  --sb:230px;
}
*{box-sizing:border-box;margin:0;padding:0;}
/* منع التمرير الأفقي من الشريط الجانبي المنزلق خارج الشاشة (يجب أن يكون على html ليشمل العناصر الثابتة) */
html{overflow-x:hidden;}
body{font-family:'Tajawal',sans-serif;background:var(--bg);color:var(--text);}

/* ── Sidebar ── */
#sgs{
  width:var(--sb);background:#fff;
  border-inline-end:1px solid var(--border);
  display:flex;flex-direction:column;
  position:fixed;top:0;inset-inline-start:0;
  height:100dvh;z-index:900;
  box-shadow:0 0 20px rgba(0,0,0,.06);
  transition:transform .28s cubic-bezier(.4,0,.2,1);
}
#sgs .sg-logo{
  padding:16px;border-bottom:1px solid var(--border);
  display:flex;align-items:center;gap:10px;flex-shrink:0;
}
#sgs .sg-icon{
  width:34px;height:34px;border-radius:10px;flex-shrink:0;
  background:linear-gradient(135deg,var(--g),var(--gm));
  display:flex;align-items:center;justify-content:center;
}
#sgs .sg-app{font-size:12px;font-weight:700;color:var(--g);line-height:1.2;}
#sgs .sg-sub{font-size:9px;color:var(--muted);}

#sgs .sg-user{
  padding:12px 16px;border-bottom:1px solid var(--border);
  flex-shrink:0;
}
#sgs .sg-row{display:flex;align-items:center;gap:10px;}
#sgs .sg-av{
  width:34px;height:34px;border-radius:50%;flex-shrink:0;
  background:linear-gradient(135deg,${rc},${rc}88);
  display:flex;align-items:center;justify-content:center;
  color:#fff;font-weight:700;font-size:14px;
}
#sgs .sg-name{font-size:12px;font-weight:600;color:var(--text);
  white-space:nowrap;overflow:hidden;text-overflow:ellipsis;max-width:140px;}
#sgs .sg-role{font-size:10px;color:var(--muted);}
#sgs .sg-airport{font-size:10px;color:var(--g);font-weight:600;margin-top:2px;}
#sgs .sg-badge{
  display:inline-flex;margin-top:6px;
  padding:2px 8px;border-radius:5px;font-size:10px;font-weight:700;
  background:${rc}18;color:${rc};
}

#sgs .sg-nav{flex:1;padding:10px;overflow-y:auto;}
#sgs .sg-nav::-webkit-scrollbar{width:3px;}
#sgs .sg-nav::-webkit-scrollbar-thumb{background:var(--border);border-radius:3px;}

#sgs .sg-sec{
  font-size:9px;font-weight:700;color:var(--muted);letter-spacing:.6px;
  text-transform:uppercase;padding:8px 10px 4px;
}
#sgs a.sg-item,#sgs button.sg-item{
  display:flex;align-items:center;gap:9px;
  padding:9px 11px;border-radius:10px;
  font-size:13px;font-weight:400;color:var(--muted);
  background:transparent;text-decoration:none;
  transition:all .15s;margin-bottom:2px;
  width:100%;border:none;cursor:pointer;
  font-family:'Tajawal',sans-serif;
  border-inline-start:3px solid transparent;
}
#sgs .sg-item:hover{background:var(--bg);color:var(--text);}
#sgs .sgs-active{
  background:var(--gl)!important;color:var(--g)!important;
  font-weight:600!important;border-inline-start-color:var(--g)!important;
}

#sgs .sg-foot{padding:10px;border-top:1px solid var(--border);flex-shrink:0;}
#sgs .sg-lang{
  display:flex;background:var(--bg);border-radius:8px;padding:3px;gap:2px;margin-bottom:8px;
}
#sgs .sg-lbtn{
  flex:1;padding:5px 0;border-radius:6px;border:none;cursor:pointer;
  font-size:11px;font-weight:600;font-family:'Tajawal',sans-serif;
  background:transparent;color:var(--muted);transition:all .2s;
}
#sgs .sg-lbtn.on{background:#fff;color:var(--g);box-shadow:0 1px 4px rgba(0,0,0,.08);}
#sgs .sg-out{
  width:100%;padding:8px;border-radius:9px;
  border:1.5px solid var(--border);background:transparent;
  color:var(--muted);cursor:pointer;font-size:12px;font-weight:600;
  font-family:'Tajawal',sans-serif;
  display:flex;align-items:center;justify-content:center;gap:6px;transition:all .15s;
}
#sgs .sg-out:hover{background:var(--redl);color:var(--red);border-color:var(--red);}

/* ── Hamburger ── */
#sgs-ham{
  display:none;position:fixed;
  top:12px;inset-inline-start:12px;z-index:1000;
  width:42px;height:42px;border-radius:12px;
  background:var(--g);border:none;cursor:pointer;
  align-items:center;justify-content:center;
  box-shadow:0 4px 14px rgba(0,108,78,.35);
}
#sgs-bg{
  display:none;position:fixed;inset:0;
  background:rgba(0,0,0,.45);z-index:899;
  backdrop-filter:blur(2px);
}

/* ── Main content offset ── */
body .main,body main{
  margin-inline-start:var(--sb)!important;
  padding:24px 28px;min-height:100dvh;
}

/* ── Mobile ── */
@media(max-width:768px){
  #sgs{transform:translateX(${dir==='rtl'?'110%':'-110%'});}
  #sgs.open{transform:translateX(0);}
  #sgs-ham{display:flex;}
  #sgs-bg.show{display:block;}
  body .main,body main{
    margin-inline-start:0!important;
    padding:16px 14px!important;
    padding-top:64px!important;
  }
}
@media(max-width:480px){
  body .main,body main{padding:12px 10px!important;padding-top:60px!important;}
}

/* ── Global notifications: all popups pinned top-center ── */
#toast{
  inset-inline:0!important; margin-inline:auto!important; width:fit-content!important; max-width:92vw!important;
  top:18px!important; bottom:auto!important;
}
#sgs-alert{
  position:fixed; top:18px; inset-inline:0; margin-inline:auto; width:fit-content; max-width:92vw; z-index:3000;
  display:flex; align-items:center; gap:9px;
  padding:12px 18px; border-radius:12px;
  font-family:'Tajawal',sans-serif; font-size:13px; font-weight:700;
  box-shadow:0 12px 34px rgba(0,0,0,.22);
  transform:translateY(-160%); opacity:0; transition:all .32s cubic-bezier(.34,1.56,.64,1);
  pointer-events:none;
}
#sgs-alert.show{ transform:translateY(0); opacity:1; }
#sgs-alert.error{ background:#DC2626; color:#fff; }
#sgs-alert.success{ background:#16A34A; color:#fff; }
#sgs-alert.info{ background:#1F2937; color:#fff; }

/* ── Shared pager (SGS.pager) ── */
.pager{ display:flex; flex-direction:column; align-items:center; gap:8px; margin-top:14px; }
.sgs-pg{ display:flex; flex-wrap:wrap; justify-content:center; gap:5px; }
.sgs-pg button{
  min-width:36px; height:36px; padding:0 10px; border-radius:9px;
  border:1.5px solid var(--border); background:#fff; color:var(--text);
  font-family:'Tajawal',sans-serif; font-size:13px; font-weight:700; cursor:pointer; transition:all .15s;
}
.sgs-pg button:hover:not(:disabled):not(.on){ border-color:var(--g); color:var(--g); }
.sgs-pg button.on{ background:var(--g); border-color:var(--g); color:#fff; cursor:default; }
.sgs-pg button:disabled{ opacity:.4; cursor:default; }
.sgs-pg .gap{ align-self:center; color:var(--muted); padding:0 2px; }
.sgs-pg-info{ font-size:12px; color:var(--muted); font-weight:600; }
/* ── Export dialog (SGS.exportExcel) ── */
#sgs-exp{ position:fixed; inset:0; z-index:2500; background:rgba(15,23,42,.5); backdrop-filter:blur(2px);
  display:flex; align-items:center; justify-content:center; padding:16px; font-family:'Tajawal',sans-serif; }
#sgs-exp .ex-box{ background:#fff; border-radius:18px; width:100%; max-width:560px; max-height:92dvh; overflow:auto;
  box-shadow:0 24px 60px rgba(0,0,0,.25); }
#sgs-exp .ex-head{ display:flex; align-items:center; gap:10px; padding:18px 20px; border-bottom:1px solid var(--border); }
#sgs-exp .ex-ico{ width:36px; height:36px; border-radius:10px; background:var(--gl); color:var(--g); display:flex; align-items:center; justify-content:center; }
#sgs-exp .ex-title{ font-size:16px; font-weight:800; color:var(--text); }
#sgs-exp .ex-sub{ font-size:12px; color:var(--muted); }
#sgs-exp .ex-x{ margin-inline-start:auto; border:none; background:transparent; font-size:22px; color:var(--muted); cursor:pointer; }
#sgs-exp .ex-body{ padding:16px 20px; display:flex; flex-direction:column; gap:14px; }
#sgs-exp .ex-l{ font-size:12px; font-weight:800; color:var(--text); margin-bottom:6px; }
#sgs-exp .ex-chips{ display:flex; flex-wrap:wrap; gap:6px; }
#sgs-exp .ex-chip{ padding:7px 12px; border-radius:9px; border:1.5px solid var(--border); background:#fff; cursor:pointer;
  font-family:inherit; font-size:12.5px; font-weight:700; color:var(--muted); display:inline-flex; align-items:center; gap:6px; }
#sgs-exp .ex-chip.on{ border-color:var(--g); background:var(--gl); color:var(--g); }
#sgs-exp .ex-chip input{ accent-color:var(--g); margin:0; }
#sgs-exp .ex-row{ display:grid; grid-template-columns:1fr 1fr; gap:10px; }
#sgs-exp input[type=date], #sgs-exp select, #sgs-exp input[type=search]{
  width:100%; padding:9px 10px; border:1.5px solid var(--border); border-radius:10px; font-family:inherit; font-size:13px; background:#fff; }
#sgs-exp .ex-sm{ font-size:11px; color:var(--muted); margin-bottom:4px; }
#sgs-exp .ex-count{ padding:11px 13px; border-radius:11px; background:var(--bg); font-size:13px; font-weight:700; color:var(--text); }
#sgs-exp .ex-count.zero{ background:var(--redl); color:var(--red); }
#sgs-exp .ex-foot{ display:flex; gap:8px; justify-content:flex-end; padding:14px 20px; border-top:1px solid var(--border); }
#sgs-exp .ex-btn{ padding:10px 18px; border-radius:10px; font-family:inherit; font-size:13px; font-weight:800; cursor:pointer; border:1.5px solid var(--border); background:#fff; color:var(--text); }
#sgs-exp .ex-btn.pri{ background:var(--g); border-color:var(--g); color:#fff; }
#sgs-exp .ex-btn:disabled{ opacity:.45; cursor:default; }
@media(max-width:480px){ #sgs-exp .ex-row{ grid-template-columns:1fr; } }
/* ── Search box in filter bars ── */
input[type=search].sgs-q{ min-width:190px; }
</style>

<button id="sgs-ham" onclick="sgsToggle()" aria-label="Menu">
  <svg width="18" height="18" fill="none" viewBox="0 0 24 24">
    <path d="M3 12h18M3 6h18M3 18h18" stroke="white" stroke-width="2.5" stroke-linecap="round"/>
  </svg>
</button>
<div id="sgs-bg" onclick="sgsToggle()"></div>

<aside id="sgs">
  <div class="sg-logo">
    <div class="sg-icon">
      <svg width="18" height="18" fill="none" viewBox="0 0 24 24">
        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="white" stroke-width="2"/>
        <polyline points="14 2 14 8 20 8" stroke="white" stroke-width="2"/>
        <line x1="16" y1="13" x2="8" y2="13" stroke="white" stroke-width="2" stroke-linecap="round"/>
        <line x1="16" y1="17" x2="8" y2="17" stroke="white" stroke-width="2" stroke-linecap="round"/>
      </svg>
    </div>
    <div>
      <div class="sg-app">${t.app}</div>
      <div class="sg-sub">${t.sub}</div>
    </div>
  </div>

  <div class="sg-user">
    <div class="sg-row">
      <div class="sg-av">${av}</div>
      <div style="overflow:hidden;min-width:0;">
        <div class="sg-name">${esc(uname)}</div>
        <div class="sg-role">${rl}</div>
        ${airInfo}
      </div>
    </div>
    <div class="sg-badge">${rl}</div>
  </div>

  <nav class="sg-nav">
    ${(showReports || showCoord || showPending || showDrafts || showReturned || showApproved) ? `<div class="sg-sec">${t.sec_ops}</div>` : ''}
    ${showReports ? `
    <a href="main.html" class="sg-item ${act('main.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M3 11.5L12 4l9 7.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="M5 10v9a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="M9 20v-6h6v6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      ${t.reports}
    </a>` : ''}
    ${showCoord ? `
    <a href="coordination.html" class="sg-item ${coordAct}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M9 11H5a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-6a2 2 0 0 0-2-2h-4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        <path d="M12 3v11m0 0l3-3m-3 3l-3-3" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      ${t.coordination}
    </a>` : ''}
    ${showPending ? `
    <a href="pending.html" class="sg-item ${act('pending.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2"/>
        <path d="M12 7v5l3 2" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      ${t.pending}
    </a>` : ''}
    ${showDrafts ? `
    <a href="drafts.html" class="sg-item ${act('drafts.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M12 20h9" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5z" stroke="currentColor" stroke-width="2" stroke-linejoin="round"/>
      </svg>
      ${t.drafts}
    </a>` : ''}
    ${showReturned ? `
    <a href="returned.html" class="sg-item ${act('returned.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M9 14L4 9l5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="M4 9h11a5 5 0 0 1 5 5 5 5 0 0 1-5 5H9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      ${t.returned}
    </a>` : ''}
    ${showApproved ? `
    <a href="approved.html" class="sg-item ${act('approved.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M9 11l3 3L22 4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      ${t.approved}
    </a>` : ''}

    ${showDash ? `
    <div class="sg-sec">${t.sec_view}</div>
    <a href="dashboard.html" class="sg-item ${act('dashboard.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <rect x="3" y="3" width="7" height="7" rx="1.5" fill="currentColor"/>
        <rect x="14" y="3" width="7" height="7" rx="1.5" fill="currentColor" opacity=".4"/>
        <rect x="3" y="14" width="7" height="7" rx="1.5" fill="currentColor" opacity=".4"/>
        <rect x="14" y="14" width="7" height="7" rx="1.5" fill="currentColor" opacity=".4"/>
      </svg>
      ${t.dashboard}
    </a>` : ''}

    ${showAdminSec ? `<div class="sg-sec">${t.sec_admin}</div>` : ''}
    ${showAdmin ? `
    <a href="admin.html" class="sg-item ${act('admin.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <circle cx="9" cy="7" r="4" stroke="currentColor" stroke-width="2"/>
        <path d="M2 21v-1a7 7 0 0 1 14 0v1" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        <path d="M16 11l2 2 4-4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      ${t.users}
    </a>` : ''}
    ${showLogs ? `
    <a href="logs.html" class="sg-item ${act('logs.html')}">
      <svg width="15" height="15" fill="none" viewBox="0 0 24 24">
        <path d="M4 4h16v16H4z" stroke="currentColor" stroke-width="2" stroke-linejoin="round"/>
        <path d="M8 8h8M8 12h8M8 16h5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      ${t.logs}
    </a>` : ''}
  </nav>

  <div class="sg-foot">
    <div class="sg-lang">
      <button class="sg-lbtn ${l==='ar'?'on':''}" onclick="sgsLang('ar')">العربية</button>
      <button class="sg-lbtn ${l==='en'?'on':''}" onclick="sgsLang('en')">English</button>
    </div>
    <button class="sg-out" onclick="sgsOut()">
      <svg width="13" height="13" fill="none" viewBox="0 0 24 24">
        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      ${t.logout}
    </button>
  </div>
</aside>`;
  }

  // ── Inject ───────────────────────────────────────
  function inject(l) {
    const old = document.getElementById('sgs-nav-root');
    if (old) old.remove();
    const div = document.createElement('div');
    div.id = 'sgs-nav-root';
    div.innerHTML = html(l);
    document.body.insertBefore(div, document.body.firstChild);
    document.documentElement.dir  = l === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = l;
  }

  // ── Global functions ─────────────────────────────
  window.sgsLang = function(l) {
    lang = l;
    S.setItem('lang', l);
    inject(l);
    if (typeof setLang === 'function') setLang(l);
  };
  window.sgsOut = function() {
    S.clear(); location.href = 'index.html';
  };
  window.sgsToggle = function() {
    const sb = document.getElementById('sgs');
    const bg = document.getElementById('sgs-bg');
    if (!sb) return;
    const open = sb.classList.toggle('open');
    bg.classList.toggle('show', open);
  };
  window.SGS = {
    API, uname, airportName: airName,
    // الصلاحيات كما حُسبت للقائمة الجانبية — مصدر واحد فلا تتناقض صفحة مع القائمة
    can,
    H:  () => ({ Authorization: 'Bearer ' + S.getItem('token') }),
    HJ: () => ({ Authorization: 'Bearer ' + S.getItem('token'), 'Content-Type': 'application/json' }),
    fmt: n => n == null ? '—' : Number(n).toLocaleString(),
    // رمز شركة الطيران من رقم الرحلة — نفس قاعدة الخادم (FlightNo.Prefix):
    // SV123 → SV · F3123 → F3 · 6E201 → 6E · SVA123 → SVA · 123 → ''
    airlineCode: f => (String(f || '').match(/^(?:[A-Z]{3}(?=[ -]?[0-9])|[A-Z]{2}|[A-Z][0-9]|[0-9][A-Z])/i) || [''])[0].toUpperCase(),
    // أقصى عدد صفوف في الصفحة الواحدة — الخادم يفرض الحد نفسه
    pageSize: 50,
    // ينتظر توقف الكتابة قبل التنفيذ (للبحث)
    debounce: (fn, ms = 350) => { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; },
    /* نافذة التصدير: يختار المستخدم ماذا يُصدَّر قبل التنزيل.
       o = { type, status, stations, airlines, airlineNames, from, to, stationCode, airline, q } — قيم الصفحة الحالية كبداية
       type: 'reports' (الافتراضي — تقارير الوصول والمغادرة) أو 'coordination' (نماذج التنسيق) */
    exportExcel(o = {}) {
      document.getElementById('sgs-exp')?.remove();
      const ar = (S.getItem('lang') || 'en') === 'ar';
      const tx = (a, e) => ar ? a : e;
      const coord = o.type === 'coordination';
      const STATUSES = coord ? [
        { v:'draft',     ar:'مسودة',  en:'Draft' },
        { v:'submitted', ar:'مكتمل', en:'Completed' },
      ] : [
        { v:'submitted', p:'reports.pending',  ar:'بانتظار الموافقة', en:'Pending approval' },
        { v:'draft',     p:'reports.drafts',   ar:'المسودات',        en:'Drafts' },
        { v:'returned',  p:'reports.returned', ar:'المُعادة',         en:'Returned' },
        { v:'approved',  p:'reports.approved', ar:'المعتمدة',        en:'Approved' },
      ].filter(x => can(x.p));
      // نوع التقرير (وصول/مغادرة) أو نوع المناولة (للتنسيق)
      const KINDS = coord ? [
        { v:'all', ar:'الكل', en:'All' }, { v:'turnaround', ar:'ذهاب وعودة', en:'Turnaround' }, { v:'transit', ar:'عبور', en:'Transit' },
        { v:'terminating', ar:'منتهية', en:'Terminating' }, { v:'originating', ar:'مُنشِئة', en:'Originating' },
      ] : [ { v:'all', ar:'الكل', en:'All' }, { v:'arrival', ar:'وصول', en:'Arrival' }, { v:'departure', ar:'مغادرة', en:'Departure' } ];
      const kindParam = coord ? 'handling' : 'kind';
      const base = coord ? `${API}/export/coordination` : `${API}/export/reports`;
      const isOn = v => coord ? (!o.status || o.status === v) : v === (o.status || 'submitted');
      const pad = n => String(n).padStart(2, '0');
      const ymd = d => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
      const today = new Date();
      const PERIODS = [
        { v:'all',    ar:'كل الفترات',   en:'All time',    r:() => ['', ''] },
        { v:'today',  ar:'اليوم',        en:'Today',       r:() => [ymd(today), ymd(today)] },
        { v:'7',      ar:'آخر ٧ أيام',   en:'Last 7 days', r:() => { const f = new Date(); f.setDate(f.getDate() - 6); return [ymd(f), ymd(today)]; } },
        { v:'month',  ar:'هذا الشهر',    en:'This month',  r:() => [ymd(new Date(today.getFullYear(), today.getMonth(), 1)), ymd(today)] },
        { v:'last',   ar:'الشهر الماضي', en:'Last month',  r:() => [ymd(new Date(today.getFullYear(), today.getMonth() - 1, 1)), ymd(new Date(today.getFullYear(), today.getMonth(), 0))] },
        { v:'year',   ar:'هذه السنة',    en:'This year',   r:() => [ymd(new Date(today.getFullYear(), 0, 1)), ymd(today)] },
        { v:'custom', ar:'فترة مخصصة',   en:'Custom',      r:null },
      ];
      const names = o.airlineNames || {};
      const airlines = [...new Set([...(o.airlines || []), 'SV', 'XY', 'F3'])].sort();
      const stations = can('stations.viewAll') ? (o.stations || []) : [];
      const chip = (name, type, v, label, on) =>
        `<label class="ex-chip ${on ? 'on' : ''}"><input type="${type}" name="${name}" value="${esc(v)}" ${on ? 'checked' : ''}>${esc(label)}</label>`;
      const st = o.stationCode ? stations.find(x => x.code === o.stationCode) : null;
      const hasRange = !!(o.from || o.to);

      const box = document.createElement('div');
      box.id = 'sgs-exp';
      box.innerHTML = `
<div class="ex-box" role="dialog" aria-modal="true">
  <div class="ex-head">
    <div class="ex-ico"><svg width="18" height="18" fill="none" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="currentColor" stroke-width="2"/><path d="M9 13l2 2 4-4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg></div>
    <div><div class="ex-title">${coord ? tx('تصدير نماذج التنسيق — Excel', 'Export coordination sheets') : tx('تصدير Excel', 'Export to Excel')}</div><div class="ex-sub">${coord ? tx('اختر النماذج المطلوبة ثم صدّرها', 'Choose which sheets to export') : tx('اختر التقارير المطلوبة ثم صدّرها', 'Choose which reports to export')}</div></div>
    <button type="button" class="ex-x" data-close aria-label="close">×</button>
  </div>
  <div class="ex-body">
    <div><div class="ex-l">${coord ? tx('نوع المناولة', 'Handling type') : tx('نوع التقرير', 'Report type')}</div><div class="ex-chips">
      ${KINDS.map(x => chip('kind', 'radio', x.v, ar ? x.ar : x.en, x.v === 'all')).join('')}
    </div></div>
    <div><div class="ex-l">${tx('الحالة', 'Status')}</div><div class="ex-chips">
      ${STATUSES.map(x => chip('status', 'checkbox', x.v, ar ? x.ar : x.en, isOn(x.v))).join('')}
    </div></div>
    <div><div class="ex-l">${tx('الفترة', 'Period')}</div><div class="ex-chips">
      ${PERIODS.map(x => chip('period', 'radio', x.v, ar ? x.ar : x.en, x.v === (hasRange ? 'custom' : 'month'))).join('')}
    </div>
      <div class="ex-row" id="ex-custom" style="margin-top:8px;${hasRange ? '' : 'display:none;'}">
        <div><div class="ex-sm">${tx('من', 'From')}</div><input type="date" id="ex-from" value="${esc(o.from || '')}"></div>
        <div><div class="ex-sm">${tx('إلى', 'To')}</div><input type="date" id="ex-to" value="${esc(o.to || '')}"></div>
      </div>
    </div>
    <div class="ex-row">
      <div><div class="ex-l">${tx('شركة الطيران', 'Airline')}</div><select id="ex-air">
        <option value="">${tx('كل الشركات', 'All airlines')}</option>
        ${airlines.map(c => `<option value="${esc(c)}" ${c === o.airline ? 'selected' : ''}>${esc(names[c] || c)}</option>`).join('')}
      </select></div>
      ${stations.length ? `<div><div class="ex-l">${tx('المطار', 'Airport')}</div><select id="ex-st">
        <option value="">${tx('كل المطارات', 'All airports')}</option>
        ${stations.map(x => `<option value="${Number(x.id)}" ${st && st.id === x.id ? 'selected' : ''}>${esc(x.code)} — ${esc(ar ? x.nameAr : x.nameEn)}</option>`).join('')}
      </select></div>` : ''}
    </div>
    <div><div class="ex-l">${tx('بحث (اختياري)', 'Search (optional)')}</div>
      <input type="search" id="ex-q" maxlength="50" value="${esc(o.q || '')}" placeholder="${tx('رقم الرحلة، الوجهة، المطار…', 'Flight, route, airport…')}"></div>
    <div class="ex-count" id="ex-count">…</div>
  </div>
  <div class="ex-foot">
    <button type="button" class="ex-btn" data-close>${tx('إلغاء', 'Cancel')}</button>
    <button type="button" class="ex-btn pri" id="ex-go" disabled>${tx('تصدير', 'Export')}</button>
  </div>
</div>`;
      document.body.appendChild(box);
      const $ = sel => box.querySelector(sel);
      const onKey = e => { if (e.key === 'Escape') close(); };
      const close = () => { box.remove(); document.removeEventListener('keydown', onKey); };
      document.addEventListener('keydown', onKey);
      box.addEventListener('click', e => { if (e.target === box || e.target.closest('[data-close]')) close(); });
      const auth = () => ({ Authorization: 'Bearer ' + S.getItem('token') });

      // الاختيارات الحالية كسلسلة استعلام
      const params = () => {
        const statuses = [...box.querySelectorAll('input[name=status]:checked')].map(i => i.value);
        const def = PERIODS.find(x => x.v === $('input[name=period]:checked').value);
        const [from, to] = def.r ? def.r() : [$('#ex-from').value, $('#ex-to').value];
        const p = new URLSearchParams({ status: statuses.join(','), [kindParam]: $('input[name=kind]:checked').value });
        if (from) p.set('from', from);
        if (to) p.set('to', to);
        if ($('#ex-air').value) p.set('airline', $('#ex-air').value);
        if ($('#ex-st') && $('#ex-st').value) p.set('stationId', $('#ex-st').value);
        if ($('#ex-q').value.trim()) p.set('q', $('#ex-q').value.trim());
        return { p, statuses };
      };

      // عدد التقارير التي ستُصدَّر — يتحدث مع كل تغيير
      let seq = 0;
      const refresh = async () => {
        const { p, statuses } = params();
        const out = $('#ex-count'), go = $('#ex-go');
        go.disabled = true;
        if (!statuses.length) { out.className = 'ex-count zero'; out.textContent = tx('اختر حالة واحدة على الأقل', 'Pick at least one status'); return; }
        const my = ++seq;
        out.className = 'ex-count'; out.textContent = tx('جارٍ الحساب…', 'Counting…');
        try {
          const res = await fetch(`${base}/count?${p}`, { headers: auth() });
          if (my !== seq) return;
          if (!res.ok) { const e = await res.json().catch(() => ({})); throw new Error(e.detail || ('HTTP ' + res.status)); }
          const d = await res.json();
          if (my !== seq) return;
          const n = v => Number(v).toLocaleString();
          out.className = 'ex-count' + (d.total ? '' : ' zero');
          out.textContent = !d.total ? tx('لا توجد نتائج مطابقة لهذه الاختيارات', 'Nothing matches these choices')
            : coord ? tx(`سيتم تصدير ${n(d.total)} نموذج تنسيق`, `${n(d.total)} coordination sheets will be exported`)
            : tx(`سيتم تصدير ${n(d.total)} تقرير  (وصول ${n(d.arrivals)} · مغادرة ${n(d.departures)})`,
                 `${n(d.total)} reports will be exported  (arrival ${n(d.arrivals)} · departure ${n(d.departures)})`);
          go.disabled = !d.total;
        } catch (e) { if (my === seq) { out.className = 'ex-count zero'; out.textContent = e.message; } }
      };
      const refreshSoon = SGS.debounce(refresh, 300);

      box.addEventListener('change', e => {
        const i = e.target;
        if (i.type === 'radio') box.querySelectorAll(`input[name=${i.name}]`).forEach(r => r.closest('.ex-chip').classList.toggle('on', r.checked));
        if (i.type === 'checkbox') i.closest('.ex-chip').classList.toggle('on', i.checked);
        if (i.name === 'period') $('#ex-custom').style.display = i.value === 'custom' ? '' : 'none';
        refreshSoon();
      });
      $('#ex-q').addEventListener('input', refreshSoon);

      $('#ex-go').addEventListener('click', async () => {
        const go = $('#ex-go');
        go.disabled = true; go.textContent = tx('جارٍ التصدير…', 'Exporting…');
        try {
          const res = await fetch(`${base}.xlsx?${params().p}`, { headers: auth() });
          if (res.status === 401) { S.clear(); location.href = 'index.html'; return; }
          if (!res.ok) { const e = await res.json().catch(() => ({})); throw new Error(e.detail || tx('تعذّر التصدير', 'Export failed')); }
          const cd = res.headers.get('Content-Disposition') || '';
          const m = cd.match(/filename\*=UTF-8''([^;]+)/i) || cd.match(/filename="?([^";]+)"?/i);
          const url = URL.createObjectURL(await res.blob());
          const a = document.createElement('a');
          a.href = url; a.download = m ? decodeURIComponent(m[1]) : 'Reports.xlsx';
          document.body.appendChild(a); a.click(); a.remove();
          setTimeout(() => URL.revokeObjectURL(url), 1500);
          close();
          window.sgsNotify(tx('تم التصدير', 'Exported'), 'success');
        } catch (e) {
          go.disabled = false; go.textContent = tx('تصدير', 'Export');
          const out = $('#ex-count'); out.className = 'ex-count zero'; out.textContent = e.message;
        }
      });
      refresh();
    },

    /* شريط الصفحات: «الأولى ‹ 1 … 4 5 [6] 7 8 … 20 › الأخيرة» + «عرض 251–300 من 1,000»
       onGo(p) يُستدعى برقم الصفحة المطلوبة */
    pager(id, page, total, size, onGo) {
      const el = document.getElementById(id);
      if (!el) return;
      const ar = (S.getItem('lang') || 'en') === 'ar';
      const pages = Math.max(1, Math.ceil(total / size));
      if (!total) { el.style.display = 'none'; el.innerHTML = ''; return; }
      el.style.display = '';
      const from = (page - 1) * size + 1, to = Math.min(total, page * size);
      const nums = [];
      for (let p = 1; p <= pages; p++)
        if (p === 1 || p === pages || Math.abs(p - page) <= 2) nums.push(p);
      let html = `<button data-p="1" ${page === 1 ? 'disabled' : ''} title="${ar ? 'الأولى' : 'First'}">«</button>`
               + `<button data-p="${page - 1}" ${page === 1 ? 'disabled' : ''}>${ar ? '›' : '‹'} ${ar ? 'السابق' : 'Prev'}</button>`;
      nums.forEach((p, i) => {
        if (i && p - nums[i - 1] > 1) html += '<span class="gap">…</span>';
        html += `<button data-p="${p}" class="${p === page ? 'on' : ''}">${p}</button>`;
      });
      html += `<button data-p="${page + 1}" ${page === pages ? 'disabled' : ''}>${ar ? 'التالي' : 'Next'} ${ar ? '‹' : '›'}</button>`
            + `<button data-p="${pages}" ${page === pages ? 'disabled' : ''} title="${ar ? 'الأخيرة' : 'Last'}">»</button>`;
      el.innerHTML = (pages > 1 ? `<div class="sgs-pg">${html}</div>` : '')
        + `<div class="sgs-pg-info">${ar ? `عرض ${from.toLocaleString()}–${to.toLocaleString()} من ${total.toLocaleString()}`
                                        : `Showing ${from.toLocaleString()}–${to.toLocaleString()} of ${total.toLocaleString()}`}</div>`;
      el.querySelectorAll('button[data-p]').forEach(b => b.onclick = () => {
        const p = +b.dataset.p;
        if (p >= 1 && p <= pages && p !== page) { onGo(p); window.scrollTo({ top: 0, behavior: 'smooth' }); }
      });
    },
  };

  // ── Global top-center notifications (popups on every page) ──
  let sgsAlertTimer;
  function sgsAlertEl() {
    let el = document.getElementById('sgs-alert');
    if (!el) { el = document.createElement('div'); el.id = 'sgs-alert'; (document.body || document.documentElement).appendChild(el); }
    return el;
  }
  window.sgsNotify = function (msg, type) {
    const el = sgsAlertEl();
    el.className = ''; el.classList.add(type || 'info');
    el.textContent = String(msg == null ? '' : msg);
    void el.offsetWidth;          // reflow to restart the transition
    el.classList.add('show');
    clearTimeout(sgsAlertTimer);
    sgsAlertTimer = setTimeout(() => el.classList.remove('show'), 3500);
  };
  // any uncaught error / rejected promise → red popup at the top
  window.addEventListener('error', e => { if (e && e.message) window.sgsNotify(e.message, 'error'); });
  window.addEventListener('unhandledrejection', e => {
    const r = e && e.reason; window.sgsNotify(r && r.message ? r.message : (r ? String(r) : 'Error'), 'error');
  });
  // fallback for pages without their own toast()
  if (!window.toast) window.toast = m => window.sgsNotify(m, 'info');

  document.addEventListener('click', e => {
    if (e.target.closest('#sgs a')) {
      document.getElementById('sgs')?.classList.remove('open');
      document.getElementById('sgs-bg')?.classList.remove('show');
    }
  });

  window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
      document.getElementById('sgs')?.classList.remove('open');
      document.getElementById('sgs-bg')?.classList.remove('show');
    }
  });

  if (document.readyState === 'loading')
    document.addEventListener('DOMContentLoaded', () => inject(lang));
  else inject(lang);
})();
