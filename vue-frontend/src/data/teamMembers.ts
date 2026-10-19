export type TeamMember = {
  slug: string
  name: string
  role: string
  roleGroup: 'Partner' | 'Attorney' | 'Paralegal' | 'Accountant' | 'LegalAssistant' | 'AdminIT'
  shortBio: string
  image: string
  background: string[]
  education?: string[]
  admissions?: string[]
}

export const teamMembers: TeamMember[] = [
  {
    slug: 'sandra-whitmore',
    name: 'Sandra Whitmore',
    role: 'Managing Partner - Litigation',
    roleGroup: 'Partner',
    shortBio:
      'Sandra leads high-stakes litigation strategy with a direct, human approach that keeps clients informed and in control.',
    image: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Raised in central Ohio and active in community legal clinics.',
      'Over 15 years in complex civil disputes and trial preparation.',
      'Mentors early-career attorneys and supports youth debate programs.',
    ],
    education: ['J.D., Ohio State University Moritz College of Law'],
    admissions: ['Ohio', 'U.S. District Court, Southern District of Ohio'],
  },
  {
    slug: 'brian-whitmore',
    name: 'Brian Whitmore',
    role: 'Founding Partner - Counsel',
    roleGroup: 'Partner',
    shortBio:
      'Brian focuses on early risk analysis, decisive legal positioning, and practical solutions that hold up in court.',
    image: 'https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Founded the firm to deliver practical legal strategy for businesses and families.',
      'Advises on dispute prevention, settlement posture, and trial-readiness.',
      'Known for clear counsel during high-pressure legal decisions.',
    ],
    education: ['J.D., Capital University Law School'],
    admissions: ['Ohio', 'U.S. District Court, Northern District of Ohio'],
  },
  {
    slug: 'alex-reed',
    name: 'Alex Reed',
    role: 'Partner - Business Litigation',
    roleGroup: 'Attorney',
    shortBio:
      'Alex counsels owners on contract conflicts, partner disputes, and risk-aware litigation strategy.',
    image: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Represents clients in commercial and contract disputes throughout Ohio.',
      'Builds early case roadmaps around leverage points and evidentiary gaps.',
      'Coordinates advisory and litigation work to reduce repeat exposure.',
    ],
  },
  {
    slug: 'morgan-lee',
    name: 'Morgan Lee',
    role: 'Senior Associate - Estate Planning',
    roleGroup: 'Attorney',
    shortBio: 'Morgan supports estate planning and probate clients with detail-oriented, practical counsel.',
    image: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Advises clients on wills, trusts, probate, and powers of attorney.',
      'Known for translating legal complexity into clear client actions.',
      'Supports contested probate matters with courtroom-ready preparation.',
    ],
  },
  {
    slug: 'jamie-clark',
    name: 'Jamie Clark',
    role: 'Lead Paralegal',
    roleGroup: 'Paralegal',
    shortBio: 'Jamie coordinates filings, hearing prep, and case timeline discipline for litigation matters.',
    image: 'https://images.unsplash.com/photo-1619895862022-09114b41f16f?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Leads court filing checklists and discovery packet organization.',
      'Maintains hearing calendars and deadline controls across active matters.',
      'Supports trial preparation from exhibit tracking to witness logistics.',
    ],
  },
  {
    slug: 'pat-rivera',
    name: 'Pat Rivera',
    role: 'Legal Assistant',
    roleGroup: 'LegalAssistant',
    shortBio: 'Pat runs consultation follow-ups, intake scheduling, and client communication handoffs.',
    image: 'https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=1000&q=80',
    background: [
      'First point of contact for consultation and intake follow-up workflows.',
      'Coordinates scheduling and case onboarding packets.',
      'Keeps communication records current across legal teams.',
    ],
  },
  {
    slug: 'taylor-ng',
    name: 'Taylor Ng',
    role: 'Accounting Manager',
    roleGroup: 'Accountant',
    shortBio: 'Taylor oversees billing operations and payment workflow reconciliation.',
    image: 'https://images.unsplash.com/photo-1556157382-97eda2d62296?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Manages invoice operations and ledger integrity checks.',
      'Coordinates finance handoff requirements for matter lifecycle stages.',
      'Supports compliance reporting for internal accounting workflows.',
    ],
  },
  {
    slug: 'casey-morgan',
    name: 'Casey Morgan',
    role: 'Admin (IT) - Systems',
    roleGroup: 'AdminIT',
    shortBio: 'Casey maintains secure operations, identity access, and platform reliability controls.',
    image: 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=1000&q=80',
    background: [
      'Owns access controls, account lifecycle, and environment reliability.',
      'Maintains secure device and data handling standards for staff workflows.',
      'Coordinates incident and audit-readiness processes for internal systems.',
    ],
  },
]

export const teamMemberBySlug = Object.fromEntries(teamMembers.map((member) => [member.slug, member])) as Record<
  string,
  TeamMember
>
