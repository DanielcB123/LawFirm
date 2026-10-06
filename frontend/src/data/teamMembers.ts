export type TeamMember = {
  slug: string;
  name: string;
  role: string;
  shortBio: string;
  image: string;
  personalHistory: string[];
  professionalHistory: string[];
  education?: string[];
  admissions?: string[];
};

export const teamMembers: TeamMember[] = [
  {
    slug: "sandra-whitmore",
    name: "Sandra Whitmore",
    role: "Managing Partner - Litigation",
    shortBio:
      "Sandra leads high-stakes litigation strategy with a direct, human approach that keeps clients informed and in control.",
    image:
      "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=1000&q=80",
    personalHistory: [
      "Raised in central Ohio in a family of educators and small business owners.",
      "Early volunteer work with community legal clinics shaped her commitment to clear, accessible legal guidance.",
      "Outside of practice, she mentors new attorneys and supports local youth debate programs.",
    ],
    professionalHistory: [
      "Over 15 years representing clients in complex civil disputes and trial preparation.",
      "Leads litigation strategy from pre-suit risk analysis through courtroom advocacy.",
      "Recognized for combining decisive legal positioning with consistent client communication.",
    ],
    education: ["J.D., The Ohio State University Moritz College of Law", "B.A., Political Science, Miami University"],
    admissions: ["Ohio", "U.S. District Court, Southern District of Ohio"],
  },
  {
    slug: "brian-whitmore",
    name: "Brian Whitmore",
    role: "Founding Partner - Counsel",
    shortBio:
      "Brian focuses on early risk analysis, decisive legal positioning, and practical solutions that hold up in court.",
    image:
      "https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=1000&q=80",
    personalHistory: [
      "Grew up in Columbus and has advised Ohio families and businesses for most of his career.",
      "Known for calm, practical counsel during high-pressure decisions.",
      "Active in local civic organizations focused on entrepreneurship and small business development.",
    ],
    professionalHistory: [
      "Founded the firm to deliver strategic, client-first legal representation.",
      "Advises on dispute prevention, settlement strategy, and trial-readiness across civil matters.",
      "Regularly supports clients through complex negotiations involving operational and reputational risk.",
    ],
    education: ["J.D., Capital University Law School", "B.S., Business Administration, Ohio University"],
    admissions: ["Ohio", "U.S. District Court, Northern District of Ohio"],
  },
  {
    slug: "jordan-parker",
    name: "Jordan Parker",
    role: "Founding Partner",
    shortBio:
      "Focuses on civil litigation and strategic dispute resolution with over 15 years of courtroom experience.",
    image:
      "https://images.unsplash.com/photo-1556157382-97eda2d62296?auto=format&fit=crop&w=1000&q=80",
    personalHistory: [
      "Supports clients through high-pressure legal conflicts with empathy and clear structure.",
      "Frequently speaks at community workshops on negotiation strategy and conflict de-escalation.",
      "Believes legal outcomes improve when clients fully understand each option in front of them.",
    ],
    professionalHistory: [
      "Represents clients in civil disputes, business conflicts, and contract-related litigation matters.",
      "Leads settlement-first case strategies while remaining fully prepared for trial when needed.",
      "Known for balancing assertive advocacy with practical, long-term planning.",
    ],
    education: ["J.D., University of Cincinnati College of Law", "B.A., Sociology, Ohio State University"],
    admissions: ["Ohio"],
  },
  {
    slug: "alex-reed",
    name: "Alex Reed",
    role: "Partner",
    shortBio:
      "Leads business litigation matters and advises owners on dispute prevention and risk management.",
    image:
      "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=1000&q=80",
    personalHistory: [
      "Comes from a family of small business operators and understands the operational reality behind legal disputes.",
      "Helps founders and operators align legal strategy with business continuity goals.",
      "Enjoys mentoring first-generation professionals exploring legal careers.",
    ],
    professionalHistory: [
      "Counsels clients on contract disputes, partnership conflicts, and commercial litigation.",
      "Builds early case maps focused on leverage points, evidentiary gaps, and settlement pathways.",
      "Coordinates litigation and advisory work to reduce repeat exposure to the same legal risks.",
    ],
    education: ["J.D., Case Western Reserve University School of Law", "B.A., Economics, Denison University"],
    admissions: ["Ohio", "U.S. District Court, Southern District of Ohio"],
  },
  {
    slug: "morgan-lee",
    name: "Morgan Lee",
    role: "Senior Associate",
    shortBio: "Supports estate planning and probate clients with clear, detail-oriented counsel.",
    image:
      "https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=1000&q=80",
    personalHistory: [
      "Known for detail-oriented planning and thoughtful client education.",
      "Works closely with families to make legal planning documents understandable and actionable.",
      "Volunteers with local nonprofit clinics focused on end-of-life planning awareness.",
    ],
    professionalHistory: [
      "Advises clients on wills, trusts, probate administration, and powers of attorney.",
      "Builds practical document strategies tailored to family structure and asset complexity.",
      "Supports contested probate matters with careful records, timelines, and court-ready preparation.",
    ],
    education: ["J.D., University of Dayton School of Law", "B.A., Communication, Ohio University"],
    admissions: ["Ohio"],
  },
];

export const teamMemberBySlug = Object.fromEntries(teamMembers.map((member) => [member.slug, member])) as Record<
  string,
  TeamMember
>;
