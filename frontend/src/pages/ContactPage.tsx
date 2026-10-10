import { type FormEvent, useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import { ApiClientError } from "../services/apiClient";
import { createConsultationRequest, getAvailableConsultationSlots } from "../services/consultationsApi";
import type { ConsultationSlot } from "../types/consultations";

const practiceAreas = [
  { value: "BusinessLitigation", label: "Business litigation" },
  { value: "CivilLitigation", label: "Civil litigation" },
  { value: "EstatePlanning", label: "Estate planning" },
  { value: "PersonalInjury", label: "Personal injury" },
  { value: "ComplianceAdvisory", label: "Risk and compliance advisory" },
];

const appointmentHours = [9, 10, 11, 12, 13, 14, 15, 16];
const appointmentMinutes = [0, 30];
const intakeChecklist = [
  "Names of all people or businesses involved",
  "Contracts, notices, or letters related to the issue",
  "Important dates and deadlines",
  "A short list of your goals and desired outcomes",
];
const consultationFaq = [
  {
    question: "How soon can I get a consultation?",
    answer: "Most requests are confirmed within one business day, with same-week availability for many matters.",
  },
  {
    question: "Can I request virtual consultation?",
    answer: "Yes. You can note virtual preference in your message and we will provide meeting details.",
  },
];

export function ContactPage() {
  const modalCardRef = useRef<HTMLElement | null>(null);
  const modalScrollTopRef = useRef<number | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [visibleMonthStart, setVisibleMonthStart] = useState(() => startOfMonth(new Date()));
  const [slots, setSlots] = useState<ConsultationSlot[]>([]);
  const [isLoadingSlots, setIsLoadingSlots] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedDayKey, setSelectedDayKey] = useState("");
  const [selectedSlot, setSelectedSlot] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [practiceArea, setPracticeArea] = useState(practiceAreas[0].value);
  const [message, setMessage] = useState("");

  useLayoutEffect(() => {
    if (!isModalOpen) {
      return;
    }

    if (modalScrollTopRef.current !== null && modalCardRef.current) {
      modalCardRef.current.scrollTop = modalScrollTopRef.current;
      modalScrollTopRef.current = null;
    }
  }, [isModalOpen, isLoadingSlots, slots]);

  const calendarRange = useMemo(() => {
    const gridStart = new Date(visibleMonthStart);
    gridStart.setDate(1 - visibleMonthStart.getDay());
    const gridEnd = new Date(gridStart);
    gridEnd.setDate(gridStart.getDate() + 41);
    return { gridStart, gridEnd, days: 42 };
  }, [visibleMonthStart]);

  const canGoToPreviousMonth = useMemo(() => {
    const currentMonth = startOfMonth(new Date());
    return visibleMonthStart.getTime() > currentMonth.getTime();
  }, [visibleMonthStart]);

  useEffect(() => {
    async function loadSlots() {
      if (!isModalOpen) {
        return;
      }

      setIsLoadingSlots(true);
      setSubmitError(null);
      try {
        const available = await getAvailableConsultationSlots({
          startDate: toIsoDate(calendarRange.gridStart),
          days: calendarRange.days,
        });
        setSlots(available);
        if (available.length > 0) {
          const firstSlot = available[0];
          setSelectedSlot(firstSlot.startsAtUtc);
          setSelectedDayKey(toLocalDayKey(firstSlot.startsAtUtc));
        } else {
          setSelectedDayKey(toLocalDayKey(calendarRange.gridStart.toISOString()));
          setSelectedSlot("");
        }
      } catch (err) {
        setSubmitError(err instanceof ApiClientError ? err.message : "Unable to load appointment times.");
      } finally {
        setIsLoadingSlots(false);
      }
    }

    void loadSlots();
  }, [calendarRange.days, calendarRange.gridStart, isModalOpen]);

  const availableSlotMapByDay = useMemo(() => {
    const grouped = new Map<string, Map<string, string>>();
    for (const slot of slots) {
      const dayKey = toLocalDayKey(slot.startsAtUtc);
      const timeKey = toLocalTimeKey(slot.startsAtUtc);
      if (!grouped.has(dayKey)) {
        grouped.set(dayKey, new Map<string, string>());
      }
      grouped.get(dayKey)?.set(timeKey, slot.startsAtUtc);
    }
    return grouped;
  }, [slots]);

  const calendarDays = useMemo(() => {
    const today = new Date();
    const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime();
    const items: { dayKey: string; date: Date; hasAvailability: boolean; inCurrentMonth: boolean; isPastDate: boolean }[] = [];

    for (let offset = 0; offset < 42; offset++) {
      const date = new Date(calendarRange.gridStart);
      date.setDate(calendarRange.gridStart.getDate() + offset);
      const dayKey = toLocalDayKey(date.toISOString());
      const dateStart = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
      const isPastDate = dateStart < todayStart;
      const hasAvailability = !isPastDate && (availableSlotMapByDay.get(dayKey)?.size ?? 0) > 0;
      items.push({
        dayKey,
        date,
        hasAvailability,
        inCurrentMonth:
          date.getMonth() === visibleMonthStart.getMonth() && date.getFullYear() === visibleMonthStart.getFullYear(),
        isPastDate,
      });
    }

    return items;
  }, [availableSlotMapByDay, calendarRange.gridStart, visibleMonthStart]);

  const activeDayKey = useMemo(() => {
    if (selectedDayKey && calendarDays.some((item) => item.dayKey === selectedDayKey)) {
      return selectedDayKey;
    }
    return calendarDays.find((item) => item.hasAvailability)?.dayKey ?? calendarDays[0]?.dayKey ?? "";
  }, [calendarDays, selectedDayKey]);

  const activeDayAvailableSlots = useMemo(() => availableSlotMapByDay.get(activeDayKey) ?? new Map<string, string>(), [
    activeDayKey,
    availableSlotMapByDay,
  ]);

  const selectedDaySlots = useMemo(() => {
    const selectedDay = calendarDays.find((item) => item.dayKey === activeDayKey)?.date;
    if (!selectedDay) {
      return [];
    }

    return appointmentHours.flatMap((hour) =>
      appointmentMinutes.map((minute) => {
        const slotDate = new Date(selectedDay.getFullYear(), selectedDay.getMonth(), selectedDay.getDate(), hour, minute, 0, 0);
        const timeKey = `${`${hour}`.padStart(2, "0")}:${`${minute}`.padStart(2, "0")}`;
        const availableUtc = activeDayAvailableSlots.get(timeKey) ?? null;
        return {
          timeKey,
          label: slotDate.toLocaleTimeString([], { hour: "numeric", minute: "2-digit" }),
          availableUtc,
        };
      }),
    );
  }, [activeDayAvailableSlots, activeDayKey, calendarDays]);

  const activeSelectedSlot = useMemo(() => {
    if (selectedDaySlots.some((slot) => slot.availableUtc === selectedSlot)) {
      return selectedSlot;
    }
    return selectedDaySlots.find((slot) => slot.availableUtc)?.availableUtc ?? "";
  }, [selectedDaySlots, selectedSlot]);

  async function submitConsultation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitError(null);
    setSubmitSuccess(null);

    if (!activeSelectedSlot) {
      setSubmitError("Please choose an available appointment time.");
      return;
    }

    setIsSubmitting(true);
    try {
      await createConsultationRequest({
        fullName,
        email,
        phone: phone || undefined,
        practiceArea,
        message: message || undefined,
        preferredAtUtc: activeSelectedSlot,
        timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC",
      });

      setSubmitSuccess("Consultation request sent. Our team will contact you shortly.");
      setFullName("");
      setEmail("");
      setPhone("");
      setMessage("");
      setPracticeArea(practiceAreas[0].value);
      setIsModalOpen(false);
    } catch (err) {
      setSubmitError(err instanceof ApiClientError ? err.message : "Unable to submit consultation request.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--contact" data-reveal="zoom">
        <p className="eyebrow">Contact us</p>
        <h1>Tell us what is happening. We will help you plan the next move.</h1>
        <p className="lead">
          Request a consultation to discuss your matter. We respond to all inquiries within one business day.
        </p>
        <button
          type="button"
          className="hero-button hero-button--primary"
          onClick={() => {
            setVisibleMonthStart(startOfMonth(new Date()));
            setIsModalOpen(true);
          }}
        >
          Request appointment
        </button>
      </section>

      <section className="content-grid marketing-grid" data-reveal="left">
        <article className="card marketing-card marketing-card--lift" data-reveal="left">
          <h2>Office</h2>
          <p>123 Justice Avenue, Suite 400</p>
          <p>Columbus, OH 43215</p>
        </article>
        <article className="card marketing-card marketing-card--lift" data-reveal>
          <h2>Phone & Email</h2>
          <p>(614) 555-0134</p>
          <p>intake@parkerreedlaw.com</p>
        </article>
        <article className="card marketing-card marketing-card--lift" data-reveal="right">
          <h2>Hours</h2>
          <p>Monday-Friday: 8:30 AM-6:00 PM</p>
          <p>Saturday: By appointment</p>
          <p>Sunday: Closed</p>
        </article>
      </section>

      <section className="marketing-highlight marketing-highlight--contact" data-reveal>
        <h2>What to include when you reach out</h2>
        <ul>
          <li>A short summary of the legal issue.</li>
          <li>Important dates, deadlines, or hearings.</li>
          <li>Any documents that help us assess urgency and next steps.</li>
        </ul>
      </section>

      <section className="content-grid marketing-grid" data-reveal="zoom">
        <article className="card marketing-card" data-reveal="left">
          <h2>Intake checklist</h2>
          <ul>
            {intakeChecklist.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Consultation FAQs</h2>
          {consultationFaq.map((item) => (
            <p key={item.question}>
              <strong>{item.question}</strong>
              <br />
              {item.answer}
            </p>
          ))}
        </article>
      </section>
      {submitSuccess && <p className="card success-text">{submitSuccess}</p>}
      {submitError && !isModalOpen && <p className="error-text">{submitError}</p>}

      {isModalOpen && (
        <div className="modal-backdrop" role="presentation" onClick={() => setIsModalOpen(false)}>
          <section
            className="modal-card"
            ref={modalCardRef}
            role="dialog"
            aria-modal="true"
            aria-labelledby="consultation-modal-title"
            onClick={(event) => event.stopPropagation()}
          >
            <h2 id="consultation-modal-title">Schedule consultation</h2>
            <p className="muted">Choose from currently available appointment times.</p>

            <form className="workspace-form" onSubmit={submitConsultation}>
              <label htmlFor="consultation-full-name">Full name</label>
              <input
                id="consultation-full-name"
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                required
              />

              <label htmlFor="consultation-email">Email</label>
              <input
                id="consultation-email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />

              <label htmlFor="consultation-phone">Phone</label>
              <input
                id="consultation-phone"
                value={phone}
                onChange={(event) => setPhone(event.target.value)}
                placeholder="Optional"
              />

              <label htmlFor="consultation-practice-area">Practice area</label>
              <select
                id="consultation-practice-area"
                value={practiceArea}
                onChange={(event) => setPracticeArea(event.target.value)}
              >
                {practiceAreas.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              <fieldset className="consultation-picker" disabled={isLoadingSlots && slots.length === 0}>
                <legend>Available appointment time</legend>
                {!isLoadingSlots && calendarDays.length === 0 && <p className="muted">No open slots right now.</p>}
                {calendarDays.length > 0 && (
                  <>
                    <div className="consultation-picker__calendar">
                      <div className="consultation-picker__calendar-header">
                        <button
                          type="button"
                          className="consultation-picker__month-nav"
                          onClick={() =>
                            canGoToPreviousMonth &&
                            (() => {
                              modalScrollTopRef.current = modalCardRef.current?.scrollTop ?? null;
                              setVisibleMonthStart(
                                (current) => new Date(current.getFullYear(), current.getMonth() - 1, 1),
                              );
                            })()
                          }
                          disabled={!canGoToPreviousMonth}
                          aria-label="Previous month"
                        >
                          ←
                        </button>
                        <p className="consultation-picker__month-label">
                          {visibleMonthStart.toLocaleDateString([], { month: "long", year: "numeric" })}
                        </p>
                        <button
                          type="button"
                          className="consultation-picker__month-nav"
                          onClick={() => {
                            modalScrollTopRef.current = modalCardRef.current?.scrollTop ?? null;
                            setVisibleMonthStart((current) => new Date(current.getFullYear(), current.getMonth() + 1, 1));
                          }}
                          aria-label="Next month"
                        >
                          →
                        </button>
                      </div>
                      <p className="consultation-picker__hint">
                        Choose a date first. Dates with no open times are shown as unavailable.
                      </p>
                      <div className="consultation-picker__weekdays" aria-hidden="true">
                        {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((weekday) => (
                          <span key={weekday}>{weekday}</span>
                        ))}
                      </div>
                      <div className="consultation-picker__calendar-grid" role="radiogroup" aria-label="Appointment dates">
                        {calendarDays.map((dayItem) => {
                          const isSelected = activeDayKey === dayItem.dayKey;
                          const isUnavailable = !dayItem.hasAvailability;
                          return (
                            <button
                              key={dayItem.dayKey}
                              type="button"
                              className={`consultation-picker__day ${isSelected ? "consultation-picker__day--selected" : ""} ${isUnavailable ? "consultation-picker__day--unavailable" : ""} ${dayItem.isPastDate ? "consultation-picker__day--past" : ""} ${!dayItem.inCurrentMonth ? "consultation-picker__day--outside-month" : ""}`}
                              onClick={() => {
                                if (isUnavailable) {
                                  return;
                                }
                                setSelectedDayKey(dayItem.dayKey);
                                const firstAvailableForDay = availableSlotMapByDay.get(dayItem.dayKey)?.values().next().value ?? "";
                                setSelectedSlot(firstAvailableForDay);
                              }}
                              disabled={isUnavailable}
                              role="radio"
                              aria-checked={isSelected}
                              aria-label={`${dayItem.date.toLocaleDateString([], { weekday: "long", month: "long", day: "numeric" })}${dayItem.isPastDate ? ", unavailable, past date" : isUnavailable ? ", unavailable" : ", available"}`}
                            >
                              <span>{dayItem.date.getDate()}</span>
                            </button>
                          );
                        })}
                      </div>
                    </div>
                    <div className="consultation-picker__time-list" role="radiogroup" aria-label="Available times">
                      {selectedDaySlots.map((slot) => {
                        const isUnavailable = !slot.availableUtc;
                        return (
                          <button
                            key={slot.timeKey}
                            type="button"
                            className={`consultation-picker__time ${activeSelectedSlot === slot.availableUtc ? "consultation-picker__time--selected" : ""} ${isUnavailable ? "consultation-picker__time--unavailable" : ""}`}
                            onClick={() => {
                              if (slot.availableUtc) {
                                setSelectedSlot(slot.availableUtc);
                              }
                            }}
                            disabled={isUnavailable}
                            role="radio"
                            aria-checked={activeSelectedSlot === slot.availableUtc}
                            aria-label={`${slot.label}${isUnavailable ? ", unavailable" : ", available"}`}
                          >
                            {slot.label}
                          </button>
                        );
                      })}
                    </div>
                    <p className="consultation-picker__selection-summary" aria-live="polite">
                      {activeSelectedSlot
                        ? `Selected: ${new Date(activeSelectedSlot).toLocaleDateString([], {
                            weekday: "long",
                            month: "long",
                            day: "numeric",
                          })} at ${new Date(activeSelectedSlot).toLocaleTimeString([], {
                            hour: "numeric",
                            minute: "2-digit",
                          })}`
                        : "No time selected yet."}
                    </p>
                  </>
                )}
              </fieldset>
              <label htmlFor="consultation-message">Message</label>
              <textarea
                id="consultation-message"
                value={message}
                onChange={(event) => setMessage(event.target.value)}
                rows={4}
                placeholder="Share context that will help us prepare for your consultation."
              />

              {submitError && <p className="error-text">{submitError}</p>}
              <div className="modal-actions">
                <button type="button" className="mini-action" onClick={() => setIsModalOpen(false)} disabled={isSubmitting}>
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting || isLoadingSlots || calendarDays.length === 0 || !activeSelectedSlot}
                >
                  {isSubmitting ? "Sending..." : "Send request"}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </section>
  );
}

function toLocalDayKey(dateValue: string): string {
  const date = new Date(dateValue);
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, "0");
  const day = `${date.getDate()}`.padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function toLocalTimeKey(dateValue: string): string {
  const date = new Date(dateValue);
  const hour = `${date.getHours()}`.padStart(2, "0");
  const minute = `${date.getMinutes()}`.padStart(2, "0");
  return `${hour}:${minute}`;
}

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, "0");
  const day = `${date.getDate()}`.padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}
