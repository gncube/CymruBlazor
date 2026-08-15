namespace CymruBlazor.Icons;

/// <summary>
/// Registry of icon SVG path data, sourced directly from the
/// <see href="https://lucide.dev">Lucide Icons</see> project (ISC licence)
/// via the <c>lucide-static</c> npm package - the same source the design
/// system's icon set ("Foundations &gt; Iconography") is built on. Values
/// are the exact inner &lt;svg&gt; content (paths/shapes only, no wrapper
/// element), extracted programmatically rather than hand-transcribed, to
/// guarantee pixel-accurate path data.
///
/// Grid: 24x24, 2px stroke, round linecap/linejoin - matching both
/// Lucide's own defaults and the design system's documented grid.
///
/// Domain grouping (<see cref="GetDomain"/>) is reconstructed from
/// screenshots of the design system's Icon Preview page, not the live
/// Figma file - names/paths are authoritative (sourced from the real
/// package), but exact per-domain membership should be spot-checked
/// against the live source if it matters for your use case.
/// </summary>
public static class IconRegistry
{
    private static readonly Dictionary<string, string> Icons = new(StringComparer.OrdinalIgnoreCase)
    {
        { "activity", "<path d=\"M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.49 12H2\" />" },
        { "add", "<path d=\"M5 12h14\" /> <path d=\"M12 5v14\" />" },
        { "add-appointment", "<path d=\"M16 19h6\" /> <path d=\"M16 2v4\" /> <path d=\"M19 16v6\" /> <path d=\"M21 12.598V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h8.5\" /> <path d=\"M3 10h18\" /> <path d=\"M8 2v4\" />" },
        { "admin-staff", "<path d=\"M10 15H6a4 4 0 0 0-4 4v2\" /> <path d=\"m14.305 16.53.923-.382\" /> <path d=\"m15.228 13.852-.923-.383\" /> <path d=\"m16.852 12.228-.383-.923\" /> <path d=\"m16.852 17.772-.383.924\" /> <path d=\"m19.148 12.228.383-.923\" /> <path d=\"m19.53 18.696-.382-.924\" /> <path d=\"m20.772 13.852.924-.383\" /> <path d=\"m20.772 16.148.924.383\" /> <circle cx=\"18\" cy=\"15\" r=\"3\" /> <circle cx=\"9\" cy=\"7\" r=\"4\" />" },
        { "alert", "<path d=\"M10.268 21a2 2 0 0 0 3.464 0\" /> <path d=\"M22 8c0-2.3-.8-4.3-2-6\" /> <path d=\"M3.262 15.326A1 1 0 0 0 4 17h16a1 1 0 0 0 .74-1.673C19.41 13.956 18 12.499 18 8A6 6 0 0 0 6 8c0 4.499-1.411 5.956-2.738 7.326\" /> <path d=\"M4 2C2.8 3.7 2 5.7 2 8\" />" },
        { "ambulance", "<path d=\"M14 18V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v11a1 1 0 0 0 1 1h2\" /> <path d=\"M15 18H9\" /> <path d=\"M19 18h2a1 1 0 0 0 1-1v-3.65a1 1 0 0 0-.22-.624l-3.48-4.35A1 1 0 0 0 17.52 8H14\" /> <circle cx=\"17\" cy=\"18\" r=\"2\" /> <circle cx=\"7\" cy=\"18\" r=\"2\" />" },
        { "anonymous", "<path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\" /> <circle cx=\"9\" cy=\"7\" r=\"4\" /> <line x1=\"17\" x2=\"22\" y1=\"8\" y2=\"13\" /> <line x1=\"22\" x2=\"17\" y1=\"8\" y2=\"13\" />" },
        { "appointment", "<path d=\"M8 2v4\" /> <path d=\"M16 2v4\" /> <rect width=\"18\" height=\"18\" x=\"3\" y=\"4\" rx=\"2\" /> <path d=\"M3 10h18\" />" },
        { "archive", "<rect width=\"20\" height=\"5\" x=\"2\" y=\"3\" rx=\"1\" /> <path d=\"M4 8v11a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8\" /> <path d=\"M10 12h4\" />" },
        { "attachment", "<path d=\"m16 6-8.414 8.586a2 2 0 0 0 2.829 2.829l8.414-8.586a4 4 0 1 0-5.657-5.657l-8.379 8.551a6 6 0 1 0 8.485 8.485l8.379-8.551\" />" },
        { "audit", "<path d=\"M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z\" /> <path d=\"m9 12 2 2 4-4\" />" },
        { "back", "<path d=\"m12 19-7-7 7-7\" /> <path d=\"M19 12H5\" />" },
        { "bed", "<path d=\"M3 20v-8a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v8\" /> <path d=\"M5 10V6a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v4\" /> <path d=\"M3 18h18\" />" },
        { "cancel-appointment", "<path d=\"M8 2v4\" /> <path d=\"M16 2v4\" /> <rect width=\"18\" height=\"18\" x=\"3\" y=\"4\" rx=\"2\" /> <path d=\"M3 10h18\" /> <path d=\"m14 14-4 4\" /> <path d=\"m10 14 4 4\" />" },
        { "carer", "<path d=\"M19.414 14.414C21 12.828 22 11.5 22 9.5a5.5 5.5 0 0 0-9.591-3.676.6.6 0 0 1-.818.001A5.5 5.5 0 0 0 2 9.5c0 2.3 1.5 4 3 5.5l5.535 5.362a2 2 0 0 0 2.879.052 2.12 2.12 0 0 0-.004-3 2.124 2.124 0 1 0 3-3 2.124 2.124 0 0 0 3.004 0 2 2 0 0 0 0-2.828l-1.881-1.882a2.41 2.41 0 0 0-3.409 0l-1.71 1.71a2 2 0 0 1-2.828 0 2 2 0 0 1 0-2.828l2.823-2.762\" />" },
        { "chart", "<path d=\"M3 3v16a2 2 0 0 0 2 2h16\" /> <path d=\"m19 9-5 5-4-4-3 3\" />" },
        { "check", "<path d=\"M20 6 9 17l-5-5\" />" },
        { "chevron-down", "<path d=\"m6 9 6 6 6-6\" />" },
        { "chevron-left", "<path d=\"m15 18-6-6 6-6\" />" },
        { "chevron-right", "<path d=\"m9 18 6-6-6-6\" />" },
        { "chevron-up", "<path d=\"m18 15-6-6-6 6\" />" },
        { "clear", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"m15 9-6 6\" /> <path d=\"m9 9 6 6\" />" },
        { "clinician", "<path d=\"m16 11 2 2 4-4\" /> <path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\" /> <circle cx=\"9\" cy=\"7\" r=\"4\" />" },
        { "clipboard-list", "<rect width=\"8\" height=\"4\" x=\"8\" y=\"2\" rx=\"1\" ry=\"1\" /> <path d=\"M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2\" /> <path d=\"M12 11h4\" /> <path d=\"M12 16h4\" /> <path d=\"M8 11h.01\" /> <path d=\"M8 16h.01\" />" },
        { "close", "<path d=\"M18 6 6 18\" /> <path d=\"m6 6 12 12\" />" },
        { "contact", "<path d=\"M16 10h2\" /> <path d=\"M16 14h2\" /> <path d=\"M6.17 15a3 3 0 0 1 5.66 0\" /> <circle cx=\"9\" cy=\"11\" r=\"2\" /> <rect x=\"2\" y=\"5\" width=\"20\" height=\"14\" rx=\"2\" />" },
        { "copy", "<rect width=\"14\" height=\"14\" x=\"8\" y=\"8\" rx=\"2\" ry=\"2\" /> <path d=\"M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2\" />" },
        { "critical", "<circle cx=\"12\" cy=\"13\" r=\"8\" /> <path d=\"M12 9v4l2 2\" /> <path d=\"M5 3 2 6\" /> <path d=\"m22 6-3-3\" /> <path d=\"M6.38 18.7 4 21\" /> <path d=\"M17.64 18.67 20 21\" />" },
        { "cross", "<rect width=\"18\" height=\"18\" x=\"3\" y=\"3\" rx=\"2\" /> <path d=\"M8 12h8\" /> <path d=\"M12 8v8\" />" },
        { "dashboard", "<rect width=\"7\" height=\"9\" x=\"3\" y=\"3\" rx=\"1\" /> <rect width=\"7\" height=\"5\" x=\"14\" y=\"3\" rx=\"1\" /> <rect width=\"7\" height=\"9\" x=\"14\" y=\"12\" rx=\"1\" /> <rect width=\"7\" height=\"5\" x=\"3\" y=\"16\" rx=\"1\" />" },
        { "delete", "<path d=\"M10 11v6\" /> <path d=\"M14 11v6\" /> <path d=\"M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6\" /> <path d=\"M3 6h18\" /> <path d=\"M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2\" />" },
        { "department", "<path d=\"M10 18v-7\" /> <path d=\"M11.119 2.205a2 2 0 0 1 1.762 0l7.84 3.846A.5.5 0 0 1 20.5 7h-17a.5.5 0 0 1-.22-.949z\" /> <path d=\"M14 18v-7\" /> <path d=\"M18 18v-7\" /> <path d=\"M3 22h18\" /> <path d=\"M6 18v-7\" />" },
        { "dna", "<path d=\"m10 16 1.5 1.5\" /> <path d=\"m14 8-1.5-1.5\" /> <path d=\"M15 2c-1.798 1.998-2.518 3.995-2.807 5.993\" /> <path d=\"m16.5 10.5 1 1\" /> <path d=\"m17 6-2.891-2.891\" /> <path d=\"M2 15c6.667-6 13.333 0 20-6\" /> <path d=\"m20 9 .891.891\" /> <path d=\"M3.109 14.109 4 15\" /> <path d=\"m6.5 12.5 1 1\" /> <path d=\"m7 18 2.891 2.891\" /> <path d=\"M9 22c1.798-1.998 2.518-3.995 2.807-5.993\" />" },
        { "document", "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\" /> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\" />" },
        { "download", "<path d=\"M12 15V3\" /> <path d=\"M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4\" /> <path d=\"m7 10 5 5 5-5\" />" },
        { "droplet", "<path d=\"M12 22a7 7 0 0 0 7-7c0-2-1-3.9-3-5.5s-3.5-4-4-6.5c-.5 2.5-2 4.9-4 6.5C6 11.1 5 13 5 15a7 7 0 0 0 7 7z\" />" },
        { "duration", "<line x1=\"10\" x2=\"14\" y1=\"2\" y2=\"2\" /> <line x1=\"12\" x2=\"15\" y1=\"14\" y2=\"11\" /> <circle cx=\"12\" cy=\"14\" r=\"8\" />" },
        { "edit", "<path d=\"M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z\" /> <path d=\"m15 5 4 4\" />" },
        { "edit2", "<path d=\"M12 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7\" /> <path d=\"M18.375 2.625a1 1 0 0 1 3 3l-9.013 9.014a2 2 0 0 1-.853.505l-2.873.84a.5.5 0 0 1-.62-.62l.84-2.873a2 2 0 0 1 .506-.852z\" />" },
        { "email", "<path d=\"m22 7-8.991 5.727a2 2 0 0 1-2.009 0L2 7\" /> <rect x=\"2\" y=\"4\" width=\"20\" height=\"16\" rx=\"2\" />" },
        { "error-circle", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <line x1=\"12\" x2=\"12\" y1=\"8\" y2=\"12\" /> <line x1=\"12\" x2=\"12.01\" y1=\"16\" y2=\"16\" />" },
        { "expand", "<path d=\"m15 15 6 6\" /> <path d=\"m15 9 6-6\" /> <path d=\"M21 16v5h-5\" /> <path d=\"M21 8V3h-5\" /> <path d=\"M3 16v5h5\" /> <path d=\"m3 21 6-6\" /> <path d=\"M3 8V3h5\" /> <path d=\"M9 9 3 3\" />" },
        { "export", "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\" /> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\" /> <path d=\"M12 18v-6\" /> <path d=\"m9 15 3 3 3-3\" />" },
        { "eye", "<path d=\"M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0\" /> <circle cx=\"12\" cy=\"12\" r=\"3\" />" },
        { "eye-off", "<path d=\"M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49\" /> <path d=\"M14.084 14.158a3 3 0 0 1-4.242-4.242\" /> <path d=\"M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143\" /> <path d=\"m2 2 20 20\" />" },
        { "file-text", "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\" /> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\" /> <path d=\"M10 9H8\" /> <path d=\"M16 13H8\" /> <path d=\"M16 17H8\" />" },
        { "filter", "<path d=\"M2 5h20\" /> <path d=\"M6 12h12\" /> <path d=\"M9 19h6\" />" },
        { "flagged", "<path d=\"M4 22V4a1 1 0 0 1 .4-.8A6 6 0 0 1 8 2c3 0 5 2 7.333 2q2 0 3.067-.8A1 1 0 0 1 20 4v10a1 1 0 0 1-.4.8A6 6 0 0 1 16 16c-3 0-5-2-8-2a6 6 0 0 0-4 1.528\" />" },
        { "flask", "<path d=\"M14 2v6a2 2 0 0 0 .245.96l5.51 10.08A2 2 0 0 1 18 22H6a2 2 0 0 1-1.755-2.96l5.51-10.08A2 2 0 0 0 10 8V2\" /> <path d=\"M6.453 15h11.094\" /> <path d=\"M8.5 2h7\" />" },
        { "folder", "<path d=\"M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z\" />" },
        { "form", "<rect width=\"8\" height=\"4\" x=\"8\" y=\"2\" rx=\"1\" ry=\"1\" /> <path d=\"M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2\" />" },
        { "forward", "<path d=\"M5 12h14\" /> <path d=\"m12 5 7 7-7 7\" />" },
        { "gp", "<path d=\"M11 2v2\" /> <path d=\"M5 2v2\" /> <path d=\"M5 3H4a2 2 0 0 0-2 2v4a6 6 0 0 0 12 0V5a2 2 0 0 0-2-2h-1\" /> <path d=\"M8 15a6 6 0 0 0 12 0v-3\" /> <circle cx=\"20\" cy=\"10\" r=\"2\" />" },
        { "gp-practice", "<path d=\"M12.35 21H5a2 2 0 0 1-2-2v-9a2 2 0 0 1 .71-1.53l7-6a2 2 0 0 1 2.58 0l7 6A2 2 0 0 1 21 10v2.35\" /> <path d=\"M14.8 12.4A1 1 0 0 0 14 12h-4a1 1 0 0 0-1 1v8\" /> <path d=\"M15 18h6\" /> <path d=\"M18 15v6\" />" },
        { "grid-2x2", "<path d=\"M12 3v18\" /> <path d=\"M3 12h18\" /> <rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\" />" },
        { "grid-3x3", "<rect width=\"18\" height=\"18\" x=\"3\" y=\"3\" rx=\"2\" /> <path d=\"M3 9h18\" /> <path d=\"M3 15h18\" /> <path d=\"M9 3v18\" /> <path d=\"M15 3v18\" />" },
        { "heart-pulse", "<path d=\"M2 9.5a5.5 5.5 0 0 1 9.591-3.676.56.56 0 0 0 .818 0A5.49 5.49 0 0 1 22 9.5c0 2.29-1.5 4-3 5.5l-5.492 5.313a2 2 0 0 1-3 .019L5 15c-1.5-1.5-3-3.2-3-5.5\" /> <path d=\"M3.22 13H9.5l.5-1 2 4.5 2-7 1.5 3.5h5.27\" />" },
        { "history", "<path d=\"M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8\" /> <path d=\"M3 3v5h5\" /> <path d=\"M12 7v5l4 2\" />" },
        { "hold", "<rect x=\"14\" y=\"3\" width=\"5\" height=\"18\" rx=\"1\" /> <rect x=\"5\" y=\"3\" width=\"5\" height=\"18\" rx=\"1\" />" },
        { "home", "<path d=\"M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8\" /> <path d=\"M3 10a2 2 0 0 1 .709-1.528l7-6a2 2 0 0 1 2.582 0l7 6A2 2 0 0 1 21 10v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\" />" },
        { "hospital", "<path d=\"M12 7v4\" /> <path d=\"M14 21v-3a2 2 0 0 0-4 0v3\" /> <path d=\"M14 9h-4\" /> <path d=\"M18 11h2a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-9a2 2 0 0 1 2-2h2\" /> <path d=\"M18 21V5a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v16\" />" },
        { "image", "<rect width=\"18\" height=\"18\" x=\"3\" y=\"3\" rx=\"2\" ry=\"2\" /> <circle cx=\"9\" cy=\"9\" r=\"2\" /> <path d=\"m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21\" />" },
        { "info", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"M12 16v-4\" /> <path d=\"M12 8h.01\" />" },
        { "language", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20\" /> <path d=\"M2 12h20\" />" },
        { "letter", "<path d=\"M21.2 8.4c.5.38.8.97.8 1.6v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V10a2 2 0 0 1 .8-1.6l8-6a2 2 0 0 1 2.4 0l8 6Z\" /> <path d=\"m22 10-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 10\" />" },
        { "link", "<path d=\"M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71\" /> <path d=\"M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71\" />" },
        { "loading", "<path d=\"M21 12a9 9 0 1 1-6.219-8.56\" />" },
        { "lock", "<rect width=\"18\" height=\"11\" x=\"3\" y=\"11\" rx=\"2\" ry=\"2\" /> <path d=\"M7 11V7a5 5 0 0 1 10 0v4\" />" },
        { "login", "<path d=\"m10 17 5-5-5-5\" /> <path d=\"M15 12H3\" /> <path d=\"M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4\" />" },
        { "logout", "<path d=\"m16 17 5-5-5-5\" /> <path d=\"M21 12H9\" /> <path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4\" />" },
        { "map-pin", "<path d=\"M20 10c0 4.993-5.539 10.193-7.399 11.799a1 1 0 0 1-1.202 0C9.539 20.193 4 14.993 4 10a8 8 0 0 1 16 0\" /> <circle cx=\"12\" cy=\"10\" r=\"3\" />" },
        { "menu", "<path d=\"M4 5h16\" /> <path d=\"M4 12h16\" /> <path d=\"M4 19h16\" />" },
        { "message", "<path d=\"M22 17a2 2 0 0 1-2 2H6.828a2 2 0 0 0-1.414.586l-2.202 2.202A.71.71 0 0 1 2 21.286V5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2z\" />" },
        { "more", "<circle cx=\"12\" cy=\"12\" r=\"1\" /> <circle cx=\"19\" cy=\"12\" r=\"1\" /> <circle cx=\"5\" cy=\"12\" r=\"1\" />" },
        { "next-of-kin", "<path d=\"M18 21a8 8 0 0 0-16 0\" /> <circle cx=\"10\" cy=\"8\" r=\"5\" /> <path d=\"M22 20c0-3.37-2-6.5-4-8a5 5 0 0 0-.45-8.3\" />" },
        { "notebook-pen", "<path d=\"M13.4 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-7.4\" /> <path d=\"M2 6h4\" /> <path d=\"M2 10h4\" /> <path d=\"M2 14h4\" /> <path d=\"M2 18h4\" /> <path d=\"M21.378 5.626a1 1 0 1 0-3.004-3.004l-5.01 5.012a2 2 0 0 0-.506.854l-.837 2.87a.5.5 0 0 0 .62.62l2.87-.837a2 2 0 0 0 .854-.506z\" />" },
        { "notification", "<path d=\"M10.268 21a2 2 0 0 0 3.464 0\" /> <path d=\"M3.262 15.326A1 1 0 0 0 4 17h16a1 1 0 0 0 .74-1.673C19.41 13.956 18 12.499 18 8A6 6 0 0 0 6 8c0 4.499-1.411 5.956-2.738 7.326\" />" },
        { "organisation", "<rect x=\"16\" y=\"16\" width=\"6\" height=\"6\" rx=\"1\" /> <rect x=\"2\" y=\"16\" width=\"6\" height=\"6\" rx=\"1\" /> <rect x=\"9\" y=\"2\" width=\"6\" height=\"6\" rx=\"1\" /> <path d=\"M5 16v-3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v3\" /> <path d=\"M12 12V8\" />" },
        { "overnight", "<path d=\"M20.985 12.486a9 9 0 1 1-9.473-9.472c.405-.022.617.46.402.803a6 6 0 0 0 8.268 8.268c.344-.215.825-.004.803.401\" />" },
        { "patient", "<path d=\"M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2\" /> <circle cx=\"12\" cy=\"7\" r=\"4\" />" },
        { "pdf", "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\" /> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\" /> <path d=\"M10 9H8\" /> <path d=\"M16 13H8\" /> <path d=\"M16 17H8\" />" },
        { "pending", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"M12 6v6l4 2\" />" },
        { "phone", "<path d=\"M13.832 16.568a1 1 0 0 0 1.213-.303l.355-.465A2 2 0 0 1 17 15h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2A18 18 0 0 1 2 4a2 2 0 0 1 2-2h3a2 2 0 0 1 2 2v3a2 2 0 0 1-.8 1.6l-.468.351a1 1 0 0 0-.292 1.233 14 14 0 0 0 6.392 6.384\" />" },
        { "pill", "<path d=\"m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\" /> <path d=\"m8.5 8.5 7 7\" />" },
        { "prescription-edit", "<path d=\"M14.364 13.634a2 2 0 0 0-.506.854l-.837 2.87a.5.5 0 0 0 .62.62l2.87-.837a2 2 0 0 0 .854-.506l4.013-4.009a1 1 0 0 0-3.004-3.004z\" /> <path d=\"M14.487 7.858A1 1 0 0 1 14 7V2\" /> <path d=\"M20 19.645V20a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l2.516 2.516\" /> <path d=\"M8 18h1\" />" },
        { "print", "<path d=\"M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2\" /> <path d=\"M6 9V3a1 1 0 0 1 1-1h10a1 1 0 0 1 1 1v6\" /> <rect x=\"6\" y=\"14\" width=\"12\" height=\"8\" rx=\"1\" />" },
        { "recurring", "<path d=\"m17 2 4 4-4 4\" /> <path d=\"M3 11v-1a4 4 0 0 1 4-4h14\" /> <path d=\"m7 22-4-4 4-4\" /> <path d=\"M21 13v1a4 4 0 0 1-4 4H3\" />" },
        { "refresh", "<path d=\"M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8\" /> <path d=\"M21 3v5h-5\" /> <path d=\"M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16\" /> <path d=\"M8 16H3v5\" />" },
        { "region", "<path d=\"M14.106 5.553a2 2 0 0 0 1.788 0l3.659-1.83A1 1 0 0 1 21 4.619v12.764a1 1 0 0 1-.553.894l-4.553 2.277a2 2 0 0 1-1.788 0l-4.212-2.106a2 2 0 0 0-1.788 0l-3.659 1.83A1 1 0 0 1 3 19.381V6.618a1 1 0 0 1 .553-.894l4.553-2.277a2 2 0 0 1 1.788 0z\" /> <path d=\"M15 5.764v15\" /> <path d=\"M9 3.236v15\" />" },
        { "remove", "<path d=\"M5 12h14\" />" },
        { "room", "<path d=\"M11 20H2\" /> <path d=\"M11 4.562v16.157a1 1 0 0 0 1.242.97L19 20V5.562a2 2 0 0 0-1.515-1.94l-4-1A2 2 0 0 0 11 4.561z\" /> <path d=\"M11 4H8a2 2 0 0 0-2 2v14\" /> <path d=\"M14 12h.01\" /> <path d=\"M22 20h-3\" />" },
        { "save", "<path d=\"M15.2 3a2 2 0 0 1 1.4.6l3.8 3.8a2 2 0 0 1 .6 1.4V19a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2z\" /> <path d=\"M17 21v-7a1 1 0 0 0-1-1H8a1 1 0 0 0-1 1v7\" /> <path d=\"M7 3v4a1 1 0 0 0 1 1h7\" />" },
        { "scan", "<path d=\"M3 7V5a2 2 0 0 1 2-2h2\" /> <path d=\"M17 3h2a2 2 0 0 1 2 2v2\" /> <path d=\"M21 17v2a2 2 0 0 1-2 2h-2\" /> <path d=\"M7 21H5a2 2 0 0 1-2-2v-2\" /> <path d=\"M7 12h10\" />" },
        { "search", "<path d=\"m21 21-4.34-4.34\" /> <circle cx=\"11\" cy=\"11\" r=\"8\" />" },
        { "send", "<path d=\"M14.536 21.686a.5.5 0 0 0 .937-.024l6.5-19a.496.496 0 0 0-.635-.635l-19 6.5a.5.5 0 0 0-.024.937l7.93 3.18a2 2 0 0 1 1.112 1.11z\" /> <path d=\"m21.854 2.147-10.94 10.939\" />" },
        { "settings", "<path d=\"M9.671 4.136a2.34 2.34 0 0 1 4.659 0 2.34 2.34 0 0 0 3.319 1.915 2.34 2.34 0 0 1 2.33 4.033 2.34 2.34 0 0 0 0 3.831 2.34 2.34 0 0 1-2.33 4.033 2.34 2.34 0 0 0-3.319 1.915 2.34 2.34 0 0 1-4.659 0 2.34 2.34 0 0 0-3.32-1.915 2.34 2.34 0 0 1-2.33-4.033 2.34 2.34 0 0 0 0-3.831A2.34 2.34 0 0 1 6.35 6.051a2.34 2.34 0 0 0 3.319-1.915\" /> <circle cx=\"12\" cy=\"12\" r=\"3\" />" },
        { "share", "<circle cx=\"18\" cy=\"5\" r=\"3\" /> <circle cx=\"6\" cy=\"12\" r=\"3\" /> <circle cx=\"18\" cy=\"19\" r=\"3\" /> <line x1=\"8.59\" x2=\"15.42\" y1=\"13.51\" y2=\"17.49\" /> <line x1=\"15.41\" x2=\"8.59\" y1=\"6.51\" y2=\"10.49\" />" },
        { "shield-alert", "<path d=\"M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z\" /> <path d=\"M12 8v4\" /> <path d=\"M12 16h.01\" />" },
        { "signed", "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\" /> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\" /> <path d=\"m9 15 2 2 4-4\" />" },
        { "sort", "<path d=\"m21 16-4 4-4-4\" /> <path d=\"M17 20V4\" /> <path d=\"m3 8 4-4 4 4\" /> <path d=\"M7 4v16\" />" },
        { "specialist", "<path d=\"m14.305 19.53.923-.382\" /> <path d=\"m15.228 16.852-.923-.383\" /> <path d=\"m16.852 15.228-.383-.923\" /> <path d=\"m16.852 20.772-.383.924\" /> <path d=\"m19.148 15.228.383-.923\" /> <path d=\"m19.53 21.696-.382-.924\" /> <path d=\"M2 21a8 8 0 0 1 10.434-7.62\" /> <path d=\"m20.772 16.852.924-.383\" /> <path d=\"m20.772 19.148.924.383\" /> <circle cx=\"10\" cy=\"8\" r=\"5\" /> <circle cx=\"18\" cy=\"18\" r=\"3\" />" },
        { "success", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"m9 12 2 2 4-4\" />" },
        { "syringe", "<path d=\"m18 2 4 4\" /> <path d=\"m17 7 3-3\" /> <path d=\"M19 9 8.7 19.3c-1 1-2.5 1-3.4 0l-.6-.6c-1-1-1-2.5 0-3.4L15 5\" /> <path d=\"m9 11 4 4\" /> <path d=\"m5 19-3 3\" /> <path d=\"m14 4 6 6\" />" },
        { "table", "<path d=\"M9 3H5a2 2 0 0 0-2 2v4m6-6h10a2 2 0 0 1 2 2v4M9 3v18m0 0h10a2 2 0 0 0 2-2V9M9 21H5a2 2 0 0 1-2-2V9m0 0h18\" />" },
        { "task", "<rect width=\"18\" height=\"18\" x=\"3\" y=\"3\" rx=\"2\" /> <path d=\"m9 12 2 2 4-4\" />" },
        { "team", "<path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\" /> <path d=\"M16 3.128a4 4 0 0 1 0 7.744\" /> <path d=\"M22 21v-2a4 4 0 0 0-3-3.87\" /> <circle cx=\"9\" cy=\"7\" r=\"4\" />" },
        { "time", "<circle cx=\"12\" cy=\"12\" r=\"10\" /> <path d=\"M12 6v6l4 2\" />" },
        { "trend-down", "<path d=\"M16 17h6v-6\" /> <path d=\"m22 17-8.5-8.5-5 5L2 7\" />" },
        { "trend-up", "<path d=\"M16 7h6v6\" /> <path d=\"m22 7-8.5 8.5-5-5L2 17\" />" },
        { "undo", "<path d=\"M9 14 4 9l5-5\" /> <path d=\"M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5a5.5 5.5 0 0 1-5.5 5.5H11\" />" },
        { "unread", "<path d=\"M12.7 3H4a2 2 0 0 0-2 2v16.286a.71.71 0 0 0 1.212.502l2.202-2.202A2 2 0 0 1 6.828 19H20a2 2 0 0 0 2-2v-4.7\" /> <circle cx=\"19\" cy=\"6\" r=\"3\" />" },
        { "upload", "<path d=\"M12 3v12\" /> <path d=\"m17 8-5-5-5 5\" /> <path d=\"M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4\" />" },
        { "urgent", "<path d=\"M16 14v2.2l1.6 1\" /> <path d=\"M16 2v4\" /> <path d=\"M21 7.5V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h3.5\" /> <path d=\"M3 10h5\" /> <path d=\"M8 2v4\" /> <circle cx=\"16\" cy=\"16\" r=\"6\" />" },
        { "waiting-list", "<path d=\"M11 5h10\" /> <path d=\"M11 12h10\" /> <path d=\"M11 19h10\" /> <path d=\"M4 4h1v5\" /> <path d=\"M4 9h2\" /> <path d=\"M6.5 20H3.4c0-1 2.6-1.925 2.6-3.5a1.5 1.5 0 0 0-2.6-1.02\" />" },
        { "ward", "<path d=\"M10 12h4\" /> <path d=\"M10 8h4\" /> <path d=\"M14 21v-3a2 2 0 0 0-4 0v3\" /> <path d=\"M6 10H4a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-2\" /> <path d=\"M6 21V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v16\" />" },
        { "ward-round", "<circle cx=\"6\" cy=\"19\" r=\"3\" /> <path d=\"M9 19h8.5a3.5 3.5 0 0 0 0-7h-11a3.5 3.5 0 0 1 0-7H15\" /> <circle cx=\"18\" cy=\"5\" r=\"3\" />" },
        { "warning", "<path d=\"m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3\" /> <path d=\"M12 9v4\" /> <path d=\"M12 17h.01\" />" },
    };

    private static readonly Dictionary<string, string> Domains = new(StringComparer.OrdinalIgnoreCase)
    {
        { "activity", "clinical-actions" },
        { "add", "clinical" },
        { "add-appointment", "schedule" },
        { "admin-staff", "people" },
        { "alert", "comms" },
        { "ambulance", "location" },
        { "anonymous", "people" },
        { "appointment", "schedule" },
        { "archive", "file" },
        { "attachment", "file" },
        { "audit", "data" },
        { "back", "nav" },
        { "bed", "location" },
        { "cancel-appointment", "schedule" },
        { "carer", "people" },
        { "chart", "data" },
        { "check", "clinical" },
        { "chevron-down", "nav" },
        { "chevron-left", "nav" },
        { "chevron-right", "nav" },
        { "chevron-up", "nav" },
        { "clear", "nav" },
        { "clinician", "people" },
        { "clipboard-list", "clinical-actions" },
        { "close", "nav" },
        { "contact", "people" },
        { "copy", "clinical" },
        { "critical", "status" },
        { "cross", "clinical-actions" },
        { "dashboard", "nav" },
        { "delete", "clinical" },
        { "department", "location" },
        { "dna", "clinical-actions" },
        { "document", "file" },
        { "download", "clinical" },
        { "droplet", "clinical-actions" },
        { "duration", "schedule" },
        { "edit", "clinical" },
        { "edit2", "clinical" },
        { "email", "comms" },
        { "error-circle", "status" },
        { "expand", "clinical-actions" },
        { "export", "data" },
        { "eye", "clinical" },
        { "eye-off", "clinical" },
        { "file-text", "clinical-actions" },
        { "filter", "nav" },
        { "flagged", "status" },
        { "flask", "clinical-actions" },
        { "folder", "file" },
        { "form", "file" },
        { "forward", "nav" },
        { "gp", "people" },
        { "gp-practice", "location" },
        { "grid-2x2", "data" },
        { "grid-3x3", "data" },
        { "heart-pulse", "clinical-actions" },
        { "history", "clinical-actions" },
        { "hold", "clinical" },
        { "home", "nav" },
        { "hospital", "location" },
        { "image", "file" },
        { "info", "status" },
        { "language", "location" },
        { "letter", "comms" },
        { "link", "clinical" },
        { "loading", "status" },
        { "lock", "clinical" },
        { "login", "clinical-actions" },
        { "logout", "clinical-actions" },
        { "map-pin", "location" },
        { "menu", "nav" },
        { "message", "comms" },
        { "more", "nav" },
        { "next-of-kin", "people" },
        { "notebook-pen", "clinical-actions" },
        { "notification", "comms" },
        { "organisation", "location" },
        { "overnight", "schedule" },
        { "patient", "people" },
        { "pdf", "file" },
        { "pending", "status" },
        { "phone", "comms" },
        { "pill", "clinical-actions" },
        { "prescription-edit", "clinical-actions" },
        { "print", "clinical" },
        { "recurring", "schedule" },
        { "refresh", "clinical" },
        { "region", "location" },
        { "remove", "clinical" },
        { "room", "location" },
        { "save", "clinical" },
        { "scan", "clinical" },
        { "search", "nav" },
        { "send", "clinical-actions" },
        { "settings", "nav" },
        { "share", "clinical" },
        { "shield-alert", "clinical-actions" },
        { "signed", "file" },
        { "sort", "nav" },
        { "specialist", "people" },
        { "success", "status" },
        { "syringe", "clinical-actions" },
        { "table", "data" },
        { "task", "comms" },
        { "team", "people" },
        { "time", "schedule" },
        { "trend-down", "data" },
        { "trend-up", "data" },
        { "undo", "clinical" },
        { "unread", "comms" },
        { "upload", "clinical" },
        { "urgent", "schedule" },
        { "waiting-list", "schedule" },
        { "ward", "location" },
        { "ward-round", "schedule" },
        { "warning", "status" },
    };

    /// <summary>
    /// Gets the inner SVG markup (paths/shapes) for the given icon name.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The name isn't registered.</exception>
    public static string GetMarkup(string name)
    {
        if (Icons.TryGetValue(name, out var markup))
        {
            return markup;
        }

        throw new KeyNotFoundException(
            $"No icon named '{name}' is registered. See {nameof(IconRegistry)}.{nameof(AllNames)} for the full list.");
    }

    /// <summary>
    /// Gets whether an icon with the given name is registered.
    /// </summary>
    public static bool Exists(string name) => Icons.ContainsKey(name);

    /// <summary>
    /// Gets the documentation domain an icon belongs to (e.g. "clinical",
    /// "nav", "status") - see the type-level remarks for a caveat on
    /// how this grouping was reconstructed.
    /// </summary>
    public static string? GetDomain(string name) =>
        Domains.TryGetValue(name, out var domain) ? domain : null;

    /// <summary>
    /// Gets every registered icon name.
    /// </summary>
    public static IReadOnlyCollection<string> AllNames => Icons.Keys;
}
