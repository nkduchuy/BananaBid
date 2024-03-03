'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import { useRouter, usePathname } from 'next/navigation'
import React from 'react'
import { LuBanana } from 'react-icons/lu'

export default function Logo() {
    const router = useRouter();
    const pathname = usePathname();
    const reset = useParamsStore(state => state.reset);

    function doReset() {
        if (pathname !== '/')
            router.push('/');

        reset();
    }

    return (
        <div onClick={doReset} className='cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500'>
            <LuBanana size={34} />
            <div>BananaBid</div>
        </div>
    )
}